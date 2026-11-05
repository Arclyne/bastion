using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchFoundEventArgs : EventArgs
{
    public MatchFoundEventArgs(int matchId, IReadOnlyList<PairedPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        MatchId = matchId;
        Players = players;
    }

    public int MatchId { get; }

    public IReadOnlyList<PairedPlayer> Players { get; }
}
