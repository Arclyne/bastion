using System;

namespace Bastion.Client.Networking.Channels;

public sealed class ServerErrorException : Exception
{
    private const string DefaultMessage = "The server could not complete the request.";

    public ServerErrorException()
        : base(DefaultMessage)
    {
    }

    public ServerErrorException(string? message)
        : base(message)
    {
    }

    public ServerErrorException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public ServerErrorException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
