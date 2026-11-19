using System;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchRejoinedEventArgs : EventArgs
{
    public MatchRejoinedEventArgs(RejoinedMatch rejoinedMatch)
    {
        ArgumentNullException.ThrowIfNull(rejoinedMatch);

        RejoinedMatch = rejoinedMatch;
    }

    public RejoinedMatch RejoinedMatch { get; }
}
