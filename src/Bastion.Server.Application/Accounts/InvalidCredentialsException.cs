using System;

namespace Bastion.Server.Application.Accounts;

public sealed class InvalidCredentialsException : Exception
{
    private const string DefaultMessage = "The identifier or the password is not valid.";

    public InvalidCredentialsException()
        : base(DefaultMessage)
    {
    }

    public InvalidCredentialsException(string? message)
        : base(message)
    {
    }

    public InvalidCredentialsException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
