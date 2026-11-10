using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matches;
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
    private const string RepositoryFailureMessage = "The database is not reachable.";

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 4, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _presenceCheckAt = _startedAt.Add(MatchmakingPolicy.PresenceCheckInterval);

    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly MatchmakingDispatcher _matchmakingDispatcher;

    public TestMatchmakingDispatcher()
    {
        var rankedMatchCreator = new RankedMatchCreator(_matchRepository, new FakeTurnOrderShuffler());
        _matchmakingDispatcher = new MatchmakingDispatcher(_matchmakingQueue, _matchmakingNotifier, rankedMatchCreator);
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));
    }

    [Fact]
    public void ReviewAndNotify_TwoWaitingPlayers_NotifiesMatchFound()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(SecondAccountId, SecondNickname, _startedAt));

        _matchmakingDispatcher.ReviewAndNotify(_startedAt);

        Assert.Single(_matchmakingNotifier.CreatedMatches);
    }

    [Fact]
    public void ReviewAndNotify_MatchNotSaved_NotifiesMatchCreationFailed()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(SecondAccountId, SecondNickname, _startedAt));
        _matchRepository.MatchSaveFailure = new MatchRepositoryException(new InvalidOperationException(RepositoryFailureMessage));

        _matchmakingDispatcher.ReviewAndNotify(_startedAt);

        Assert.Equal(SearchEndReason.MatchCreationFailed, _matchmakingNotifier.EndReasons[FirstAccountId]);
    }

    [Fact]
    public void ReviewAndNotify_FifteenMinutesWaiting_RequestsPresence()
    {
        _matchmakingDispatcher.ReviewAndNotify(_presenceCheckAt);

        Assert.Single(_matchmakingNotifier.PresenceRequests);
    }

    [Fact]
    public void ReviewAndNotify_PresenceUnanswered_NotifiesPresenceExpired()
    {
        _matchmakingDispatcher.ReviewAndNotify(_presenceCheckAt);

        _matchmakingDispatcher.ReviewAndNotify(_presenceCheckAt.Add(MatchmakingPolicy.PresenceResponseWindow));

        Assert.Equal(SearchEndReason.PresenceExpired, _matchmakingNotifier.EndReasons[FirstAccountId]);
    }
}
