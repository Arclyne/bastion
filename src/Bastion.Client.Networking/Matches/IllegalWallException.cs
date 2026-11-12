using System;

namespace Bastion.Client.Networking.Matches;

public sealed class IllegalWallException : Exception
{
    public IllegalWallException(WallPlacementRejection rejection, byte? blockedTurnOrder, Exception innerException)
        : base($"The wall was rejected. Rejection={rejection}; BlockedTurn={blockedTurnOrder}", innerException)
    {
        Rejection = rejection;
        BlockedTurnOrder = blockedTurnOrder;
    }

    public WallPlacementRejection Rejection { get; }

    public byte? BlockedTurnOrder { get; }
}
