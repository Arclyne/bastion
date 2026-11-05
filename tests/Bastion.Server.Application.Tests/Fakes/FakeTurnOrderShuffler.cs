using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeTurnOrderShuffler : ITurnOrderShuffler
{
    public bool ShouldReverseOrder { get; set; }

    public IReadOnlyList<MatchSearch> ShuffleTurnOrder(IReadOnlyList<MatchSearch> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        if (!ShouldReverseOrder)
        {
            return players;
        }

        return Enumerable.Reverse(players).ToArray();
    }
}
