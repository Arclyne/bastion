using System.Collections.Generic;

namespace Bastion.Server.Application.Matchmaking;

public sealed record CreatedMatch(int MatchId, IReadOnlyList<CreatedMatchParticipant> Participants);
