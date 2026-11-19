using System;

namespace Bastion.Server.Application.Matches;

public sealed record MatchDrawFollowUp
{
    public MatchDrawFollowUp(IMatchCloser closer, IMatchDrawNotifier drawNotifier)
    {
        ArgumentNullException.ThrowIfNull(closer);
        ArgumentNullException.ThrowIfNull(drawNotifier);

        Closer = closer;
        DrawNotifier = drawNotifier;
    }

    public IMatchCloser Closer { get; }

    public IMatchDrawNotifier DrawNotifier { get; }
}
