using Bastion.Domain.GameModes;

namespace Bastion.Server.Application.Rooms;

public sealed record RequestedRoomSettings(GameModeCode GameMode, byte WallsPerPlayer, byte ClockMinutes);
