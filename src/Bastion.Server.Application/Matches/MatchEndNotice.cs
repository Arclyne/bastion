using System.Collections.Generic;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed record MatchEndNotice
{
    public required MatchEndReason EndReason { get; init; }

    public required byte DecidingTurnOrder { get; init; }

    public required bool IsRanked { get; init; }

    public required int MoveCount { get; init; }

    public required int PlayedSeconds { get; init; }

    public required IReadOnlyList<PlayerStanding> Standings { get; init; }
}
