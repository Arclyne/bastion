using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Adapters.Matchmaking;

public sealed class TurnOrderShuffler : ITurnOrderShuffler
{
    public IReadOnlyList<MatchSearch> ShuffleTurnOrder(IReadOnlyList<MatchSearch> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        MatchSearch[] shuffledPlayers = players.ToArray();
        Random.Shared.Shuffle(shuffledPlayers);
        return shuffledPlayers;
    }
}
