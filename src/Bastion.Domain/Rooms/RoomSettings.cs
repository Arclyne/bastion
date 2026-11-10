using System;
using Bastion.Domain.GameModes;

namespace Bastion.Domain.Rooms;

public sealed record RoomSettings
{
    public RoomSettings(GameMode gameMode, byte wallsPerPlayer, byte clockMinutes)
    {
        ArgumentNullException.ThrowIfNull(gameMode);

        GameMode = gameMode;
        WallsPerPlayer = wallsPerPlayer;
        ClockMinutes = clockMinutes;
    }

    public GameMode GameMode { get; }

    public byte WallsPerPlayer { get; init; }

    public byte ClockMinutes { get; }

    public bool IsValid()
    {
        return GameMode.IsActive
            && RoomPolicy.IsValidWallCount(WallsPerPlayer)
            && RoomPolicy.IsValidClock(ClockMinutes);
    }
}
