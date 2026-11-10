using System;
using System.Collections.Generic;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed record QueueReview
{
    public QueueReview(
        IReadOnlyList<MatchSearch> expiredSearches,
        IReadOnlyList<MatchPairing> pairings,
        IReadOnlyList<PresenceRequest> presenceRequests)
    {
        ArgumentNullException.ThrowIfNull(expiredSearches);
        ArgumentNullException.ThrowIfNull(pairings);
        ArgumentNullException.ThrowIfNull(presenceRequests);

        ExpiredSearches = expiredSearches;
        Pairings = pairings;
        PresenceRequests = presenceRequests;
    }

    public IReadOnlyList<MatchSearch> ExpiredSearches { get; }

    public IReadOnlyList<MatchPairing> Pairings { get; }

    public IReadOnlyList<PresenceRequest> PresenceRequests { get; }
}
