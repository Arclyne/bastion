using System;

namespace Bastion.Server.Application.Matches;

public sealed class MatchStoreException : Exception
{
    private const string DefaultMessage = "The match store could not complete the operation.";

    public MatchStoreException()
        : base(DefaultMessage)
    {
    }

    public MatchStoreException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public MatchStoreException(string? message)
        : base(message)
    {
    }

    public MatchStoreException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
