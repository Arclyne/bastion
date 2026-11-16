using System;
using System.Linq;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Screens.Matches;

public sealed record JoinedMatch
{
    public JoinedMatch(LoggedInSession session, MatchFoundEventArgs foundMatch)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(foundMatch);

        Session = session;
        FoundMatch = foundMatch;
    }

    public LoggedInSession Session { get; }

    public MatchFoundEventArgs FoundMatch { get; }

    public bool IsJoinedMatch(int matchId)
    {
        return matchId == FoundMatch.MatchId;
    }

    public byte GetBoardSize()
    {
        return FoundMatch.Format.BoardSize;
    }

    public byte GetOwnTurnOrder()
    {
        return FindOwnParticipant().TurnOrder;
    }

    public FoundMatchParticipant FindOwnParticipant()
    {
        return FoundMatch.Participants.First(IsOwnParticipant);
    }

    public bool IsOwnParticipant(FoundMatchParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        return string.Equals(participant.Nickname, Session.Nickname, StringComparison.Ordinal);
    }
}
