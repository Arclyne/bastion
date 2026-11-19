using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedParticipationEnd(
    int MatchId,
    IReadOnlyCollection<int> RecipientAccountIds,
    ParticipationEndNotice Notice);
