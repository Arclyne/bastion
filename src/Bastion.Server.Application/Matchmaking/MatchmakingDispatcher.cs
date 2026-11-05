using System;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingDispatcher
{
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public MatchmakingDispatcher(IMatchmakingQueue matchmakingQueue, IMatchmakingNotifier matchmakingNotifier)
    {
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);

        _matchmakingQueue = matchmakingQueue;
        _matchmakingNotifier = matchmakingNotifier;
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
            _matchmakingNotifier.NotifyMatchFound(pairing);
        }

        foreach (PresenceRequest request in review.PresenceRequests)
        {
            _matchmakingNotifier.NotifyPresenceRequested(request);
        }
    }
}
