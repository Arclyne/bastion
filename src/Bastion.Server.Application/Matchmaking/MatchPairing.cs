using System;
using System.Collections.Generic;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed record MatchPairing
{
    public MatchPairing(IReadOnlyList<MatchSearch> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        Players = players;
    }

    public IReadOnlyList<MatchSearch> Players { get; }
}
