using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedMatchAbandonment(
    int MatchId,
    IReadOnlyCollection<int> RecipientAccountIds,
    AbandonmentNotice Notice);
