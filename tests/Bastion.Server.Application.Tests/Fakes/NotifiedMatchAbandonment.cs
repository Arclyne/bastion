using System.Collections.Generic;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedMatchAbandonment(
    int MatchId,
    IReadOnlyCollection<int> RecipientAccountIds,
    string AbandoningNickname);
