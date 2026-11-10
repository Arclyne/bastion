using System;
using System.Collections.Generic;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matchmaking;

public sealed record CreatedMatch
{
    public CreatedMatch(int matchId, MatchFormat format, IReadOnlyList<CreatedMatchParticipant> participants)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(participants);

        MatchId = matchId;
        Format = format;
        Participants = participants;
    }

    public int MatchId { get; }

    public MatchFormat Format { get; }

    public IReadOnlyList<CreatedMatchParticipant> Participants { get; }
}
