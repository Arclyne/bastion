using System;

namespace Bastion.Client.Networking.Matches;

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

    public MatchNotJoinedException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
