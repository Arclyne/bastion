using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(Exception innerException)
        : base("The identifier or the password is not valid.", innerException)
    {
    }
}
