using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Adapters.Matchmaking;

public sealed class TurnOrderShuffler : ITurnOrderShuffler
{
    public IReadOnlyList<TPlayer> ShuffleTurnOrder<TPlayer>(IReadOnlyList<TPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        TPlayer[] shuffledPlayers = players.ToArray();
        Random.Shared.Shuffle(shuffledPlayers);
        return shuffledPlayers;
    }
}
