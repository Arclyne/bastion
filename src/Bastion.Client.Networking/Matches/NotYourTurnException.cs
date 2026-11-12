using System;

namespace Bastion.Client.Networking.Matches;

public sealed class NotYourTurnException : Exception
{
    public NotYourTurnException(byte currentTurnOrder, short nextMoveNumber, Exception innerException)
        : base($"Another player has the turn. CurrentTurnOrder={currentTurnOrder}", innerException)
    {
        CurrentTurnOrder = currentTurnOrder;
        NextMoveNumber = nextMoveNumber;
    }

    public byte CurrentTurnOrder { get; }

    public short NextMoveNumber { get; }
}
