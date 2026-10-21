using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class ConnectionLostException : Exception
{
    public ConnectionLostException(Exception innerException)
        : base("The connection with the server was lost.", innerException)
    {
    }
}
