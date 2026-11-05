using System;

namespace Bastion.Server.Application.Matches;

public sealed class MatchNotJoinedException : Exception
{
    private const string DefaultMessage = "The player is not playing that match.";

    public MatchNotJoinedException()
        : base(DefaultMessage)
    {
    }

    public MatchNotJoinedException(string? message)
        : base(message)
    {
    }

    public MatchNotJoinedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
