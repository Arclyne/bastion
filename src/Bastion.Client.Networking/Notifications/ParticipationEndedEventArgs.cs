using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Networking.Notifications;

public sealed class ParticipationEndedEventArgs : EventArgs
{
    public required int MatchId { get; init; }

    public required EndedMatchReason EndReason { get; init; }

    public required byte DepartingTurnOrder { get; init; }

    public required int ActivePlayerCount { get; init; }

    public required byte NextTurnOrder { get; init; }

    public required short NextMoveNumber { get; init; }

    public required int MoveCount { get; init; }

    public required int PlayedSeconds { get; init; }

    public required IReadOnlyList<PlayerStanding> Standings { get; init; }
}
