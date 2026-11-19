using System.Collections.Generic;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed record ParticipationEndNotice
{
    public required MatchEndReason EndReason { get; init; }

    public required byte DepartingTurnOrder { get; init; }

    public required int ActivePlayerCount { get; init; }

    public required byte NextTurnOrder { get; init; }

    public required short NextMoveNumber { get; init; }

    public required int MoveCount { get; init; }

    public required int PlayedSeconds { get; init; }

    public required IReadOnlyList<PlayerStanding> Standings { get; init; }
}
