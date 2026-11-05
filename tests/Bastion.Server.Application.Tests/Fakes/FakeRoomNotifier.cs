using System.Collections.Generic;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomNotifier : IRoomNotifier
{
    public List<NotifiedRoom> UpdatedRooms { get; } = new List<NotifiedRoom>();

    public List<NotifiedRoom> ClosedRooms { get; } = new List<NotifiedRoom>();

    public void NotifyRoomUpdated(RoomSnapshot room, int originAccountId)
    {
        UpdatedRooms.Add(new NotifiedRoom(room, originAccountId));
    }

    public void NotifyRoomClosed(RoomSnapshot room, int originAccountId)
    {
        ClosedRooms.Add(new NotifiedRoom(room, originAccountId));
    }
}
