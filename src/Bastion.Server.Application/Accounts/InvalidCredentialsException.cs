using System;

namespace Bastion.Server.Application.Accounts;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("The identifier or the password is not valid.")
    {
    }
}
