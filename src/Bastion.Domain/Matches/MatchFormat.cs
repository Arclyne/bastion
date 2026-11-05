using Bastion.Domain.GameModes;

namespace Bastion.Domain.Matches;

public sealed record MatchFormat(GameMode GameMode, byte ClockMinutes)
{
    public byte WallsPerPlayer { get; init; } = GameMode.WallsPerPlayer;
}
