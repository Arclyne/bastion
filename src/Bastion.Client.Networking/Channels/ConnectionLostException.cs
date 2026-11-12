using System;

namespace Bastion.Client.Networking.Channels;

public sealed class ConnectionLostException : Exception
{
    private const string DefaultMessage = "The connection with the server was lost.";

    public ConnectionLostException()
        : base(DefaultMessage)
    {
    }

    public ConnectionLostException(string? message)
        : base(message)
    {
    }

    public ConnectionLostException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public ConnectionLostException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
