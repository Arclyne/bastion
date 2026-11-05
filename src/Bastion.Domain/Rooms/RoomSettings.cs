using Bastion.Domain.GameModes;

namespace Bastion.Domain.Rooms;

public sealed record RoomSettings(GameMode GameMode, byte WallsPerPlayer, byte ClockMinutes)
{
    public bool IsValid()
    {
        return GameMode.IsActive
            && RoomRules.IsValidWallCount(WallsPerPlayer)
            && RoomRules.IsValidClock(ClockMinutes);
    }
}
