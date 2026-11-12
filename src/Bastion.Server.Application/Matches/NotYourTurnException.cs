using System;

namespace Bastion.Server.Application.Matches;

public sealed class NotYourTurnException : Exception
{
    public NotYourTurnException(byte currentTurnOrder, short nextMoveNumber)
        : base($"Another player has the turn or the move is out of order. CurrentTurn={currentTurnOrder}; "
            + $"NextMoveNumber={nextMoveNumber}")
    {
        CurrentTurnOrder = currentTurnOrder;
        NextMoveNumber = nextMoveNumber;
    }

    public byte CurrentTurnOrder { get; }

    public short NextMoveNumber { get; }
}
