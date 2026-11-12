using System;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed class IllegalWallException : Exception
{
    public IllegalWallException(WallRejectionReason reason, byte? blockedTurnOrder)
        : base($"The wall cannot be placed. Reason={reason}; BlockedTurn={blockedTurnOrder}")
    {
        Reason = reason;
        BlockedTurnOrder = blockedTurnOrder;
    }

    public WallRejectionReason Reason { get; }

    public byte? BlockedTurnOrder { get; }
}
