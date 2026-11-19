using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedMatchEnd(int MatchId, IReadOnlyCollection<int> RecipientAccountIds, MatchEndNotice Notice);
