using System;

namespace Bastion.Server.Application.Matches;

public sealed record MatchConnectionFollowUp
{
    public MatchConnectionFollowUp(IMatchCloser closer, IMatchConnectionNotifier connectionNotifier, IMatchGate gate)
    {
        ArgumentNullException.ThrowIfNull(closer);
        ArgumentNullException.ThrowIfNull(connectionNotifier);
        ArgumentNullException.ThrowIfNull(gate);

        Closer = closer;
        ConnectionNotifier = connectionNotifier;
        Gate = gate;
    }

    public IMatchCloser Closer { get; }

    public IMatchConnectionNotifier ConnectionNotifier { get; }

    public IMatchGate Gate { get; }
}
