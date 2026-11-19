using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchEndedEventArgs : EventArgs
{
    public required int MatchId { get; init; }

    public required EndedMatchReason EndReason { get; init; }

    public required byte DecidingTurnOrder { get; init; }

    public required bool IsRanked { get; init; }

    public required int MoveCount { get; init; }

    public required int PlayedSeconds { get; init; }

    public required IReadOnlyList<PlayerStanding> Standings { get; init; }
}
