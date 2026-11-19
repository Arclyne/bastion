using System;

namespace Bastion.Server.Application.Matches;

public sealed record MatchMoveFollowUp
{
    public MatchMoveFollowUp(IMatchMoveNotifier moveNotifier, IMatchCloser closer, IMatchDrawNotifier drawNotifier)
    {
        ArgumentNullException.ThrowIfNull(moveNotifier);
        ArgumentNullException.ThrowIfNull(closer);
        ArgumentNullException.ThrowIfNull(drawNotifier);

        MoveNotifier = moveNotifier;
        Closer = closer;
        DrawNotifier = drawNotifier;
    }

    public IMatchMoveNotifier MoveNotifier { get; }

    public IMatchCloser Closer { get; }

    public IMatchDrawNotifier DrawNotifier { get; }
}
