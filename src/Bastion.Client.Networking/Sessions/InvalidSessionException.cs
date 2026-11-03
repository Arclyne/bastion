using System;

namespace Bastion.Client.Networking.Sessions;

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

    public InvalidSessionException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
