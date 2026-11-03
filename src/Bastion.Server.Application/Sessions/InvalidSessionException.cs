using System;

namespace Bastion.Server.Application.Sessions;

public sealed class InvalidSessionException : Exception
{
    private const string DefaultMessage = "The session is not valid.";

    public InvalidSessionException()
        : base(DefaultMessage)
    {
    }

    public InvalidSessionException(string? message)
        : base(message)
    {
    }

    public InvalidSessionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
