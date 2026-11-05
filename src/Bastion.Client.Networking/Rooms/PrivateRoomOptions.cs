namespace Bastion.Client.Networking.Rooms;

public sealed record PrivateRoomOptions(PrivateRoomGameMode GameMode, byte WallsPerPlayer, byte ClockMinutes);
