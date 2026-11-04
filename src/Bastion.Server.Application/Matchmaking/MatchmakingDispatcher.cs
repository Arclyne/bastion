using System;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingDispatcher
{
    private readonly IMatchmakingQueue _queue;
    private readonly IMatchmakingNotifier _notifier;

    public MatchmakingDispatcher(IMatchmakingQueue queue, IMatchmakingNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(notifier);

        _queue = queue;
        _notifier = notifier;
    }

    public void Review(DateTime now)
    {
        QueueReview review = _queue.Review(now);
        foreach (MatchSearch search in review.ExpiredSearches)
        {
            _notifier.NotifySearchEnded(search.AccountId, SearchEndReason.PresenceExpired);
        }

        foreach (MatchPairing pairing in review.Pairings)
        {
            _notifier.NotifyMatchFound(pairing);
        }

        foreach (PresenceRequest request in review.PresenceRequests)
        {
            _notifier.RequestPresence(request);
        }
    }
}
