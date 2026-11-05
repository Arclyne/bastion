using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchFoundEventArgs : EventArgs
{
    public MatchFoundEventArgs(int matchId, FoundMatchFormat format, IReadOnlyList<FoundMatchParticipant> participants)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(participants);

        MatchId = matchId;
        Format = format;
        Participants = participants;
    }

    public int MatchId { get; }

    public FoundMatchFormat Format { get; }

    public IReadOnlyList<FoundMatchParticipant> Participants { get; }
}
