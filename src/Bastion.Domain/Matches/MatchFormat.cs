using System;
using Bastion.Domain.GameModes;

namespace Bastion.Domain.Matches;

public sealed record MatchFormat
{
    public MatchFormat(GameMode gameMode, byte clockMinutes)
    {
        ArgumentNullException.ThrowIfNull(gameMode);

        GameMode = gameMode;
        ClockMinutes = clockMinutes;
        WallsPerPlayer = gameMode.WallsPerPlayer;
    }

    public GameMode GameMode { get; }

    public byte ClockMinutes { get; }

    public byte WallsPerPlayer { get; init; }
}
