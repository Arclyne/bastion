using System;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public class TestMatchSearchService
{
    private const int AccountId = 7;
    private const string Nickname = "marta_muros";
    private const string Token = "marta-token";
    private const string UnknownToken = "unknown-token";
    private const int RepeatedStartMinutes = 10;
    private const int RoomId = 1;
    private const string RoomCode = "K7MTQ3";

    private static readonly DateTime _now = new DateTime(2026, 11, 4, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _presenceCheckAt = _now.Add(MatchmakingPolicy.PresenceCheckInterval);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly FakeMatchStore _matchStore = new FakeMatchStore();
    private readonly RoomRegistry _roomRegistry = new RoomRegistry(new FakeRoomCodeGenerator());
    private readonly MatchSearchService _matchSearchService;

    public TestMatchSearchService()
    {
        var playerActivity = new PlayerActivity(_matchmakingNotifier, _matchmakingQueue, _roomRegistry);
        var playerVerifier = new MatchmakingPlayerVerifier(_sessionVerifier, playerActivity, _matchStore);
        _matchSearchService = new MatchSearchService(playerVerifier, _matchmakingQueue, _matchmakingNotifier);
        _sessionVerifier.Add(Token, new ConnectedPlayer(AccountId, Nickname));
        _matchmakingNotifier.SubscribedAccountIds.Add(AccountId);
    }

    [Fact]
    public void StartSearch_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() => _matchSearchService.StartSearch(UnknownToken, _now));
    }

    [Fact]
    public void StartSearch_PlayerWithoutNotifications_ThrowsSearchRejected()
    {
        _matchmakingNotifier.SubscribedAccountIds.Clear();

        Assert.Throws<SearchRejectedException>(() => _matchSearchService.StartSearch(Token, _now));
    }

    [Fact]
    public void StartSearch_PlayerWithUnfinishedMatch_ThrowsSearchRejected()
    {
        _matchStore.AccountIdsWithUnfinishedParticipation.Add(AccountId);

        Assert.Throws<SearchRejectedException>(() => _matchSearchService.StartSearch(Token, _now));
    }

    [Fact]
    public void StartSearch_PlayerInRoom_RejectsInRoom()
    {
        var settings = new RoomSettings(
            GameModeSamples.CreateClassic(),
            GameModeSamples.ClassicWallsPerPlayer,
            MatchmakingPolicy.RankedClockMinutes);
        var host = new RoomMember(AccountId, Nickname);
        _roomRegistry.TryOpen(new WaitingRoom(new RoomIdentity(RoomId, RoomCode), settings, host));

        SearchRejectedException rejection =
            Assert.Throws<SearchRejectedException>(() => _matchSearchService.StartSearch(Token, _now));

        Assert.Equal(SearchRejection.InRoom, rejection.Rejection);
    }

    [Fact]
    public void StartSearch_SubscribedPlayer_JoinsQueue()
    {
        _matchSearchService.StartSearch(Token, _now);

        bool isRemoved = _matchmakingQueue.TryRemove(AccountId);

        Assert.True(isRemoved);
    }

    [Fact]
    public void StartSearch_PlayerAlreadySearching_KeepsFirstSearch()
    {
        _matchSearchService.StartSearch(Token, _now);
        _matchSearchService.StartSearch(Token, _now.AddMinutes(RepeatedStartMinutes));

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt);

        Assert.Single(review.PresenceRequests);
    }

    [Fact]
    public void CancelSearch_SearchingPlayer_NotifiesCancelled()
    {
        _matchSearchService.StartSearch(Token, _now);

        _matchSearchService.CancelSearch(Token, _now);

        Assert.Equal(SearchEndReason.Cancelled, _matchmakingNotifier.EndReasons[AccountId]);
    }

    [Fact]
    public void CancelSearch_PlayerNotSearching_NotifiesNothing()
    {
        _matchSearchService.CancelSearch(Token, _now);

        Assert.Empty(_matchmakingNotifier.EndReasons);
    }

    [Fact]
    public void ConfirmPresence_PlayerAwaitingPresence_KeepsSearching()
    {
        _matchSearchService.StartSearch(Token, _now);
        _matchmakingQueue.Review(_presenceCheckAt);
        _matchSearchService.ConfirmPresence(Token, _presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt.Add(MatchmakingPolicy.PresenceResponseWindow));

        Assert.Empty(review.ExpiredSearches);
    }
}
