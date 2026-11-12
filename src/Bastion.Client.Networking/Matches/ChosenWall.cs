namespace Bastion.Client.Networking.Matches;

public sealed record ChosenWall
{
    public required int MatchId { get; init; }

    public required short MoveNumber { get; init; }

    public required string Groove { get; init; }

    public required PlacedWallOrientation Orientation { get; init; }
}
