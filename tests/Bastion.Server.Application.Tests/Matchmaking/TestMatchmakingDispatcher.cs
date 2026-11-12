using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public sealed class TestMatchmakingDispatcher
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
    private readonly FakeTurnOrderShuffler _turnOrderShuffler = new FakeTurnOrderShuffler();
    private readonly MatchmakingDispatcher _matchmakingDispatcher;

    public TestMatchmakingDispatcher()
    {
        var rankedMatchCreator = new RankedMatchCreator(_matchRepository, _turnOrderShuffler);
        _matchmakingDispatcher = new MatchmakingDispatcher(_matchmakingQueue, _matchmakingNotifier, rankedMatchCreator);
        var firstSearch = new MatchSearch(FirstAccountId, FirstNickname, _startedAt);
        _matchmakingQueue.TryAdd(firstSearch);
    }

    [Fact]
    public void ReviewAndNotify_TwoWaitingPlayers_NotifiesMatchFound()
    {
        AddSecondSearch();

        _matchmakingDispatcher.ReviewAndNotify(_startedAt);

        Assert.Single(_matchmakingNotifier.CreatedMatches);
    }

    [Fact]
    public void ReviewAndNotify_MatchNotSaved_NotifiesMatchCreationFailed()
    {
        AddSecondSearch();
        var cause = new InvalidOperationException(RepositoryFailureMessage);
        _matchRepository.MatchSaveFailure = new MatchRepositoryException(cause);

        _matchmakingDispatcher.ReviewAndNotify(_startedAt);

        SearchEndReason reason = _matchmakingNotifier.EndReasons[FirstAccountId];
        Assert.Equal(SearchEndReason.MatchCreationFailed, reason);
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
        DateTime responseDeadline = _presenceCheckAt.Add(MatchmakingPolicy.PresenceResponseWindow);

        _matchmakingDispatcher.ReviewAndNotify(responseDeadline);

        SearchEndReason reason = _matchmakingNotifier.EndReasons[FirstAccountId];
        Assert.Equal(SearchEndReason.PresenceExpired, reason);
    }

    private void AddSecondSearch()
    {
        var secondSearch = new MatchSearch(SecondAccountId, SecondNickname, _startedAt);
        _matchmakingQueue.TryAdd(secondSearch);
    }
}
