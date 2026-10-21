using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class ServerUnreachableException : Exception
{
    public ServerUnreachableException(Exception innerException)
        : base("The server could not be reached.", innerException)
    {
    }
}
