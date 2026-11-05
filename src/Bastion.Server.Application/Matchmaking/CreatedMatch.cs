using System.Collections.Generic;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matchmaking;

public sealed record CreatedMatch(
    int MatchId,
    MatchFormat Format,
    IReadOnlyList<CreatedMatchParticipant> Participants);
