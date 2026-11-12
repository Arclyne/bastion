using System;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Networking.Notifications;

public sealed class WallPlacedEventArgs : EventArgs
{
    public required int MatchId { get; init; }

    public required string PlacerNickname { get; init; }

    public required string Groove { get; init; }

    public required PlacedWallOrientation Orientation { get; init; }

    public required byte RemainingWalls { get; init; }

    public required byte NextTurnOrder { get; init; }

    public required short NextMoveNumber { get; init; }

    public required int? RemainingClockMilliseconds { get; init; }
}
