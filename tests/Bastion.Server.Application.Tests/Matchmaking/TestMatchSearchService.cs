using System;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public sealed class TestMatchSearchService
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
    private static readonly ConnectedPlayer _player = new ConnectedPlayer(AccountId, Nickname);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeRoomCodeGenerator _codeGenerator = new FakeRoomCodeGenerator();
    private readonly RoomRegistry _roomRegistry;
    private readonly MatchSearchService _matchSearchService;

    public TestMatchSearchService()
    {
        _roomRegistry = new RoomRegistry(_codeGenerator);
        var playerActivity = new PlayerActivity(_matchmakingNotifier, _matchmakingQueue, _roomRegistry);
        var playerVerifier = new MatchmakingPlayerVerifier(_sessionVerifier, playerActivity, _matchRepository);
        _matchSearchService = new MatchSearchService(playerVerifier, _matchmakingQueue, _matchmakingNotifier);
    }

    [Fact]
    public void StartSearch_InvalidSession_ThrowsInvalidSession()
    {
        Action start = () => _matchSearchService.StartSearch(UnknownToken, _now);

        Assert.Throws<InvalidSessionException>(start);
    }

    [Fact]
    public void StartSearch_PlayerWithoutNotifications_ThrowsSearchRejected()
    {
        _sessionVerifier.Add(Token, _player);

        Action start = () => _matchSearchService.StartSearch(Token, _now);

        Assert.Throws<SearchRejectedException>(start);
    }

    [Fact]
    public void StartSearch_PlayerWithUnfinishedMatch_ThrowsSearchRejected()
    {
        AddPlayer();
        _matchRepository.AccountIdsWithUnfinishedParticipation.Add(AccountId);

        Action start = () => _matchSearchService.StartSearch(Token, _now);

        Assert.Throws<SearchRejectedException>(start);
    }

    [Fact]
    public void StartSearch_PlayerInRoom_RejectsInRoom()
    {
        AddPlayer();
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var settings = new RoomSettings(classicMode, classicMode.WallsPerPlayer, MatchmakingPolicy.RankedClockMinutes);
        var identity = new RoomIdentity(RoomId, RoomCode);
        var host = new RoomMember(AccountId, Nickname);
        var room = new WaitingRoom(identity, settings, host);
        _roomRegistry.TryOpen(room);
        Action start = () => _matchSearchService.StartSearch(Token, _now);

        Exception? exception = Record.Exception(start);

        var rejection = exception as SearchRejectedException;
        Assert.Equal(SearchRejection.InRoom, rejection?.Rejection);
    }

    [Fact]
    public void StartSearch_SubscribedPlayer_JoinsQueue()
    {
        AddPlayer();

        _matchSearchService.StartSearch(Token, _now);

        bool isRemoved = _matchmakingQueue.TryRemove(AccountId);
        Assert.True(isRemoved);
    }

    [Fact]
    public void StartSearch_PlayerAlreadySearching_KeepsFirstSearch()
    {
        AddPlayer();
        _matchSearchService.StartSearch(Token, _now);
        DateTime repeatedStartAt = _now.AddMinutes(RepeatedStartMinutes);

        _matchSearchService.StartSearch(Token, repeatedStartAt);

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt);
        Assert.Single(review.PresenceRequests);
    }

    [Fact]
    public void CancelSearch_SearchingPlayer_NotifiesCancelled()
    {
        AddPlayer();
        _matchSearchService.StartSearch(Token, _now);

        _matchSearchService.CancelSearch(Token, _now);

        SearchEndReason reason = _matchmakingNotifier.EndReasons[AccountId];
        Assert.Equal(SearchEndReason.Cancelled, reason);
    }

    [Fact]
    public void CancelSearch_PlayerNotSearching_NotifiesNothing()
    {
        AddPlayer();

        _matchSearchService.CancelSearch(Token, _now);

        Assert.Empty(_matchmakingNotifier.EndReasons);
    }

    [Fact]
    public void ConfirmPresence_PlayerAwaitingPresence_KeepsSearching()
    {
        AddPlayer();
        _matchSearchService.StartSearch(Token, _now);
        _matchmakingQueue.Review(_presenceCheckAt);
        DateTime responseDeadline = _presenceCheckAt.Add(MatchmakingPolicy.PresenceResponseWindow);

        _matchSearchService.ConfirmPresence(Token, _presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(responseDeadline);
        Assert.Empty(review.ExpiredSearches);
    }

    private void AddPlayer()
    {
        _sessionVerifier.Add(Token, _player);
        _matchmakingNotifier.SubscribedAccountIds.Add(AccountId);
    }
}
