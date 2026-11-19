using System;

namespace Bastion.Server.Application.Matches;

public sealed class DrawNotPendingException : Exception
{
    private const string DefaultMessage = "There is no draw offer to answer.";

    public DrawNotPendingException()
        : base(DefaultMessage)
    {
    }

    public DrawNotPendingException(string? message)
        : base(message)
    {
    }

    public DrawNotPendingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
