using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class ServerUnreachableException : Exception
{
    private const string DefaultMessage = "The server could not be reached.";

    public ServerUnreachableException()
        : base(DefaultMessage)
    {
    }

    public ServerUnreachableException(string? message)
        : base(message)
    {
    }

    public ServerUnreachableException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public ServerUnreachableException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
