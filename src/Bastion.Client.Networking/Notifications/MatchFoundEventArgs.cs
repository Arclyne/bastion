using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchFoundEventArgs : EventArgs
{
    public MatchFoundEventArgs(IReadOnlyList<string> participantNicknames)
    {
        ArgumentNullException.ThrowIfNull(participantNicknames);

        ParticipantNicknames = participantNicknames;
    }

    public IReadOnlyList<string> ParticipantNicknames { get; }
}
