using System;

namespace Bastion.Server.Application.Rooms;

public sealed record RoomDeparture
{
    public RoomDeparture(RoomSnapshot room, bool isRoomClosed)
    {
        ArgumentNullException.ThrowIfNull(room);

        Room = room;
        IsRoomClosed = isRoomClosed;
    }

    public RoomSnapshot Room { get; }

    public bool IsRoomClosed { get; }
}
