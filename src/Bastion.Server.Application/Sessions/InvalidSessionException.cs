using System;

namespace Bastion.Server.Application.Sessions;

public sealed class InvalidSessionException : Exception
{
    public InvalidSessionException()
        : base("The session is not valid.")
    {
    }
}
