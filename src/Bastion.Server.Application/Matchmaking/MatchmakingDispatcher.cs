using System;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingDispatcher
{
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly IMatchmakingNotifier _matchmakingNotifier;
    private readonly IRankedMatchCreator _rankedMatchCreator;

    public MatchmakingDispatcher(
        IMatchmakingQueue matchmakingQueue,
        IMatchmakingNotifier matchmakingNotifier,
        IRankedMatchCreator rankedMatchCreator)
    {
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);
        ArgumentNullException.ThrowIfNull(rankedMatchCreator);

        _matchmakingQueue = matchmakingQueue;
        _matchmakingNotifier = matchmakingNotifier;
        _rankedMatchCreator = rankedMatchCreator;
    }

    public void ReviewAndNotify(DateTime now)
    {
        QueueReview review = _matchmakingQueue.Review(now);
        foreach (MatchSearch search in review.ExpiredSearches)
        {
            _matchmakingNotifier.NotifySearchEnded(search.AccountId, SearchEndReason.PresenceExpired);
        }

        foreach (MatchPairing pairing in review.Pairings)
        {
            StartRankedMatch(pairing, now);
        }

        foreach (PresenceRequest request in review.PresenceRequests)
        {
            _matchmakingNotifier.NotifyPresenceRequested(request);
        }
    }

    private void StartRankedMatch(MatchPairing pairing, DateTime now)
    {
        if (!_rankedMatchCreator.TryCreateMatch(pairing, now, out CreatedMatch? createdMatch))
        {
            NotifyMatchCreationFailed(pairing);
            return;
        }

        _matchmakingNotifier.NotifyMatchFound(createdMatch);
    }

    private void NotifyMatchCreationFailed(MatchPairing pairing)
    {
        foreach (MatchSearch search in pairing.Players)
        {
            _matchmakingNotifier.NotifySearchEnded(search.AccountId, SearchEndReason.MatchCreationFailed);
        }
    }
}
