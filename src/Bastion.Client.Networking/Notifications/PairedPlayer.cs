using System;

namespace Bastion.Client.Networking.Notifications;

public sealed record PairedPlayer
{
    public PairedPlayer(string nickname, short? eloRating, byte turnOrder)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        Nickname = nickname;
        EloRating = eloRating;
        TurnOrder = turnOrder;
    }

    public string Nickname { get; }

    public short? EloRating { get; }

    public byte TurnOrder { get; }
}
