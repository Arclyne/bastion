namespace Bastion.Client.Networking.Matches;

public sealed record RejoinedMove
{
    public required short MoveNumber { get; init; }

    public required byte TurnOrder { get; init; }

    public required bool IsWall { get; init; }

    public required string Groove { get; init; }

    public required PlacedWallOrientation Orientation { get; init; }
}
