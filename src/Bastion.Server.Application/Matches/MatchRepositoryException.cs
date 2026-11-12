using System;

namespace Bastion.Server.Application.Matches;

public sealed class MatchRepositoryException : Exception
{
    private const string DefaultMessage = "The match repository could not complete the operation.";

    public MatchRepositoryException()
        : base(DefaultMessage)
    {
    }

    public MatchRepositoryException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public MatchRepositoryException(string? message)
        : base(message)
    {
    }

    public MatchRepositoryException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
