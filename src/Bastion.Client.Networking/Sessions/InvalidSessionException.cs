using System;

namespace Bastion.Client.Networking.Sessions;

public sealed class InvalidSessionException : Exception
{
    public InvalidSessionException(Exception innerException)
        : base("The session is not valid.", innerException)
    {
    }
}
