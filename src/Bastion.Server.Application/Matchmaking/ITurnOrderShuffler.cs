using System.Collections.Generic;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public interface ITurnOrderShuffler
{
    IReadOnlyList<MatchSearch> ShuffleTurnOrder(IReadOnlyList<MatchSearch> players);
}
