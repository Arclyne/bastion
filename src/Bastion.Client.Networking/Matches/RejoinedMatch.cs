using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matches;

public sealed record RejoinedMatch
{
    public required MatchFoundEventArgs FoundMatch { get; init; }

    public required IReadOnlyList<RejoinedPlayer> Players { get; init; }

    public required IReadOnlyList<RejoinedMove> Moves { get; init; }

    public required byte CurrentTurnOrder { get; init; }

    public required short NextMoveNumber { get; init; }

    public required TimeSpan TurnElapsed { get; init; }

    public required byte PendingDrawOffererTurnOrder { get; init; }

    public required int DrawMovesLeft { get; init; }

    public MatchEndedEventArgs? Ending { get; init; }

    public ParticipationEndedEventArgs? OwnDeparture { get; init; }
}
