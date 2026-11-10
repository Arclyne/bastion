using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

public sealed record CreatedMatchParticipant
{
    public CreatedMatchParticipant(ConnectedPlayer player, short? eloRating, byte turnOrder)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        EloRating = eloRating;
        TurnOrder = turnOrder;
    }

    public ConnectedPlayer Player { get; }

    public short? EloRating { get; }

    public byte TurnOrder { get; }
}
