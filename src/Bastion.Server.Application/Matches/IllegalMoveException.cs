using System;

namespace Bastion.Server.Application.Matches;

public sealed class IllegalMoveException : Exception
{
    private const string DefaultMessage = "The pawn cannot reach that square.";

    public IllegalMoveException()
        : base(DefaultMessage)
    {
    }

    public IllegalMoveException(string? message)
        : base(message)
    {
    }

    public IllegalMoveException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
