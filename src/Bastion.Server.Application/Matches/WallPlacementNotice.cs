using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed record WallPlacementNotice
{
    public required string PlacerNickname { get; init; }

    public required string Groove { get; init; }

    public required WallOrientation Orientation { get; init; }

    public required byte RemainingWalls { get; init; }

    public required byte NextTurnOrder { get; init; }

    public required short NextMoveNumber { get; init; }

    public required int? RemainingClockMilliseconds { get; init; }
}
