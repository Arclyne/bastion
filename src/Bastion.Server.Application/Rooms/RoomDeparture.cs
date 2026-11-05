namespace Bastion.Server.Application.Rooms;

public sealed record RoomDeparture(RoomSnapshot Room, int LeavingAccountId, bool IsRoomClosed);
