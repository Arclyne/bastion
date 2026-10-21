using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class ServerErrorException : Exception
{
    public ServerErrorException(Exception innerException)
        : base("The server could not complete the request.", innerException)
    {
    }
}
