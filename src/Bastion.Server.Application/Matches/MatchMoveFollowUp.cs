using System;

namespace Bastion.Server.Application.Matches;

public sealed record MatchMoveFollowUp
{
    public MatchMoveFollowUp(IMatchMoveNotifier moveNotifier, IMatchCloser closer)
    {
        ArgumentNullException.ThrowIfNull(moveNotifier);
        ArgumentNullException.ThrowIfNull(closer);

        MoveNotifier = moveNotifier;
        Closer = closer;
    }

    public IMatchMoveNotifier MoveNotifier { get; }

    public IMatchCloser Closer { get; }
}
