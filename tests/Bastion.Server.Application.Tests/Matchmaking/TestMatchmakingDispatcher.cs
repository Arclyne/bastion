using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public class TestMatchmakingDispatcher
{
    private const int FirstAccountId = 1;
    private const int SecondAccountId = 2;
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "leo_peon";

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 4, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _presenceCheckAt = _startedAt.Add(MatchmakingRules.PresenceCheckInterval);

    private readonly MatchmakingQueue _queue = new MatchmakingQueue();
    private readonly FakeMatchmakingNotifier _notifier = new FakeMatchmakingNotifier();
    private readonly MatchmakingDispatcher _dispatcher;

    public TestMatchmakingDispatcher()
    {
        _dispatcher = new MatchmakingDispatcher(_queue, _notifier);
        _queue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));
    }

    [Fact]
    public void Review_TwoWaitingPlayers_NotifiesMatchFound()
    {
        _queue.TryAdd(new MatchSearch(SecondAccountId, SecondNickname, _startedAt));

        _dispatcher.Review(_startedAt);

        Assert.Single(_notifier.Pairings);
    }

    [Fact]
    public void Review_FifteenMinutesWaiting_RequestsPresence()
    {
        _dispatcher.Review(_presenceCheckAt);

        Assert.Single(_notifier.PresenceRequests);
    }

    [Fact]
    public void Review_PresenceUnanswered_NotifiesPresenceExpired()
    {
        _dispatcher.Review(_presenceCheckAt);

        _dispatcher.Review(_presenceCheckAt.Add(MatchmakingRules.PresenceResponseWindow));

        Assert.Equal(SearchEndReason.PresenceExpired, _notifier.EndReasons[FirstAccountId]);
    }
}
