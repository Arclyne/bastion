using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedWallPlacement(
    int MatchId,
    IReadOnlyCollection<int> RecipientAccountIds,
    WallPlacementNotice Notice);
