using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matchmaking;
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

    private static readonly DateTime _now = new DateTime(2026, 11, 4, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _presenceCheckAt = _now.Add(MatchmakingRules.PresenceCheckInterval);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly MatchmakingQueue _queue = new MatchmakingQueue();
    private readonly FakeMatchmakingNotifier _notifier = new FakeMatchmakingNotifier();
    private readonly MatchSearchService _service;

    public TestMatchSearchService()
    {
        _service = new MatchSearchService(_sessionVerifier, _queue, _notifier);
        _sessionVerifier.Add(Token, new ConnectedPlayer(AccountId, Nickname));
        _notifier.SubscribedAccountIds.Add(AccountId);
    }

    [Fact]
    public void Start_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() => _service.Start(UnknownToken, _now));
    }

    [Fact]
    public void Start_PlayerWithoutNotifications_ThrowsSearchRejected()
    {
        _notifier.SubscribedAccountIds.Clear();

        Assert.Throws<SearchRejectedException>(() => _service.Start(Token, _now));
    }

    [Fact]
    public void Start_SubscribedPlayer_JoinsQueue()
    {
        _service.Start(Token, _now);

        bool isRemoved = _queue.TryRemove(AccountId);

        Assert.True(isRemoved);
    }

    [Fact]
    public void Start_PlayerAlreadySearching_KeepsFirstSearch()
    {
        _service.Start(Token, _now);
        _service.Start(Token, _now.AddMinutes(RepeatedStartMinutes));

        QueueReview review = _queue.Review(_presenceCheckAt);

        Assert.Single(review.PresenceRequests);
    }

    [Fact]
    public void Cancel_SearchingPlayer_NotifiesCancelled()
    {
        _service.Start(Token, _now);

        _service.Cancel(Token, _now);

        Assert.Equal(SearchEndReason.Cancelled, _notifier.EndReasons[AccountId]);
    }

    [Fact]
    public void Cancel_PlayerNotSearching_NotifiesNothing()
    {
        _service.Cancel(Token, _now);

        Assert.Empty(_notifier.EndReasons);
    }

    [Fact]
    public void ConfirmPresence_PlayerAwaitingPresence_KeepsSearching()
    {
        _service.Start(Token, _now);
        _queue.Review(_presenceCheckAt);
        _service.ConfirmPresence(Token, _presenceCheckAt);

        QueueReview review = _queue.Review(_presenceCheckAt.Add(MatchmakingRules.PresenceResponseWindow));

        Assert.Empty(review.ExpiredSearches);
    }
}
