using System.Collections.Generic;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed record QueueReview(
    IReadOnlyList<MatchSearch> ExpiredSearches,
    IReadOnlyList<MatchPairing> Pairings,
    IReadOnlyList<PresenceRequest> PresenceRequests);
