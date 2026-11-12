using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed record ChosenWall
{
    public required int MatchId { get; init; }

    public required short MoveNumber { get; init; }

    public required string Groove { get; init; }

    public required WallOrientation Orientation { get; init; }
}
