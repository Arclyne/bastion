using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matchmaking;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public class TestMatchmakingQueue
{
    private const int FirstAccountId = 1;
    private const int SecondAccountId = 2;
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "leo_peon";
    private static readonly DateTime _startedAt = new DateTime(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _presenceCheckAt = _startedAt.Add(MatchmakingRules.PresenceCheckInterval);
    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();

    [Fact]
    public void TryAdd_PlayerAlreadySearching_ReturnsFalse()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));

        bool isAdded = _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));

        Assert.False(isAdded);
    }

    [Fact]
    public void Review_TwoWaitingPlayers_PairsThem()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));
        _matchmakingQueue.TryAdd(new MatchSearch(SecondAccountId, SecondNickname, _startedAt));

        QueueReview review = _matchmakingQueue.Review(_startedAt);

        Assert.Single(review.Pairings);
    }

    [Fact]
    public void Review_FifteenMinutesWaiting_RequestsPresence()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt);

        Assert.Single(review.PresenceRequests);
    }

    [Fact]
    public void Review_PlayerAwaitingPresence_IsNotPaired()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));
        _matchmakingQueue.Review(_presenceCheckAt);
        _matchmakingQueue.TryAdd(new MatchSearch(SecondAccountId, SecondNickname, _presenceCheckAt));

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt);

        Assert.Empty(review.Pairings);
    }

    [Fact]
    public void Review_PresenceUnanswered_EndsSearch()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));
        _matchmakingQueue.Review(_presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt.Add(MatchmakingRules.PresenceResponseWindow));

        Assert.Single(review.ExpiredSearches);
    }

    [Fact]
    public void Review_PresenceConfirmed_KeepsSearching()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(FirstAccountId, FirstNickname, _startedAt));
        _matchmakingQueue.Review(_presenceCheckAt);
        _matchmakingQueue.ConfirmPresence(FirstAccountId, _presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt.Add(MatchmakingRules.PresenceResponseWindow));

        Assert.Empty(review.ExpiredSearches);
    }
}
