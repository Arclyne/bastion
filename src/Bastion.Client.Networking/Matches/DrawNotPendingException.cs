using System;

namespace Bastion.Client.Networking.Matches;

public sealed class DrawNotPendingException : Exception
{
    private const string DefaultMessage = "There is no draw offer to answer.";

    public DrawNotPendingException()
        : base(DefaultMessage)
    {
    }

    public DrawNotPendingException(string? message)
        : base(message)
    {
    }

    public DrawNotPendingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public DrawNotPendingException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
    }
}
