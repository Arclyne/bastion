using System.Collections.Generic;

namespace Bastion.Server.Application.Matchmaking;

public interface ITurnOrderShuffler
{
    IReadOnlyList<TPlayer> ShuffleTurnOrder<TPlayer>(IReadOnlyList<TPlayer> players);
}
