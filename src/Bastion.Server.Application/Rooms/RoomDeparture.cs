using System;

namespace Bastion.Server.Application.Rooms;

public sealed record RoomDeparture
{
    public RoomDeparture(RoomSnapshot room, int leavingAccountId, bool isRoomClosed)
    {
        ArgumentNullException.ThrowIfNull(room);

        Room = room;
        LeavingAccountId = leavingAccountId;
        IsRoomClosed = isRoomClosed;
    }

    public RoomSnapshot Room { get; }

    public int LeavingAccountId { get; }

    public bool IsRoomClosed { get; }
}
