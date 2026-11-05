using System.Collections.Generic;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed record MatchPairing(IReadOnlyList<MatchSearch> Players);
