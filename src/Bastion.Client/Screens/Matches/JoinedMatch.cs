using System;
using System.Linq;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Screens.Matches;

public sealed record JoinedMatch(LoggedInSession Session, MatchFoundEventArgs FoundMatch)
{
    public FoundMatchParticipant FindOwnParticipant()
    {
        return FoundMatch.Participants.First(IsOwnParticipant);
    }

    public FoundMatchParticipant FindRivalParticipant()
    {
        return FoundMatch.Participants.First(participant => !IsOwnParticipant(participant));
    }

    public bool IsOwnParticipant(FoundMatchParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        return string.Equals(participant.Nickname, Session.Nickname, StringComparison.Ordinal);
    }
}
