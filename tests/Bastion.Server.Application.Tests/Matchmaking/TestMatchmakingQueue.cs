using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matchmaking;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public sealed class TestMatchmakingQueue
{
    private const int FirstAccountId = 1;
    private const int SecondAccountId = 2;
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "leo_peon";

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _presenceCheckAt = _startedAt.Add(MatchmakingPolicy.PresenceCheckInterval);
    private static readonly DateTime _responseDeadline = _presenceCheckAt.Add(MatchmakingPolicy.PresenceResponseWindow);

    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();

    [Fact]
    public void TryAdd_PlayerAlreadySearching_ReturnsFalse()
    {
        AddFirstSearch();
        var repeatedSearch = new MatchSearch(FirstAccountId, FirstNickname, _startedAt);

        bool isAdded = _matchmakingQueue.TryAdd(repeatedSearch);

        Assert.False(isAdded);
    }

    [Fact]
    public void Review_TwoWaitingPlayers_PairsThem()
    {
        AddFirstSearch();
        AddSecondSearch(_startedAt);

        QueueReview review = _matchmakingQueue.Review(_startedAt);

        Assert.Single(review.Pairings);
    }

    [Fact]
    public void Review_FifteenMinutesWaiting_RequestsPresence()
    {
        AddFirstSearch();

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt);

        Assert.Single(review.PresenceRequests);
    }

    [Fact]
    public void Review_PlayerAwaitingPresence_IsNotPaired()
    {
        AddFirstSearch();
        _matchmakingQueue.Review(_presenceCheckAt);
        AddSecondSearch(_presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(_presenceCheckAt);

        Assert.Empty(review.Pairings);
    }

    [Fact]
    public void Review_PresenceUnanswered_EndsSearch()
    {
        AddFirstSearch();
        _matchmakingQueue.Review(_presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(_responseDeadline);

        Assert.Single(review.ExpiredSearches);
    }

    [Fact]
    public void Review_PresenceConfirmed_KeepsSearching()
    {
        AddFirstSearch();
        _matchmakingQueue.Review(_presenceCheckAt);
        _matchmakingQueue.ConfirmPresence(FirstAccountId, _presenceCheckAt);

        QueueReview review = _matchmakingQueue.Review(_responseDeadline);

        Assert.Empty(review.ExpiredSearches);
    }

    private void AddFirstSearch()
    {
        var firstSearch = new MatchSearch(FirstAccountId, FirstNickname, _startedAt);
        _matchmakingQueue.TryAdd(firstSearch);
    }

    private void AddSecondSearch(DateTime startedAt)
    {
        var secondSearch = new MatchSearch(SecondAccountId, SecondNickname, startedAt);
        _matchmakingQueue.TryAdd(secondSearch);
    }
}
