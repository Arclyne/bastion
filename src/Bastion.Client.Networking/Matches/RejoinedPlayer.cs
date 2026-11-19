using System;

namespace Bastion.Client.Networking.Matches;

public sealed record RejoinedPlayer
{
    public required byte TurnOrder { get; init; }

    public required string Square { get; init; }

    public required byte RemainingWalls { get; init; }

    public required TimeSpan RemainingClock { get; init; }

    public required bool HasEnded { get; init; }

    public required bool IsConnected { get; init; }

    public required TimeSpan TimeAway { get; init; }
}
