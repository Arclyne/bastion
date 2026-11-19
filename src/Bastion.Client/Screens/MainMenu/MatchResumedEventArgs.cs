using System;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MatchResumedEventArgs : EventArgs
{
    public MatchResumedEventArgs(RejoinedMatch rejoinedMatch)
    {
        ArgumentNullException.ThrowIfNull(rejoinedMatch);

        RejoinedMatch = rejoinedMatch;
    }

    public RejoinedMatch RejoinedMatch { get; }
}
