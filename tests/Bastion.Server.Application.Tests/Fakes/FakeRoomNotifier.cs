using System.Collections.Generic;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomNotifier : IRoomNotifier
{
    public List<NotifiedRoom> UpdatedRooms { get; } = new List<NotifiedRoom>();

    public List<NotifiedRoom> ClosedRooms { get; } = new List<NotifiedRoom>();

    public void NotifyRoomUpdated(RoomSnapshot room, int originAccountId)
    {
        var updatedRoom = new NotifiedRoom(room, originAccountId);
        UpdatedRooms.Add(updatedRoom);
    }

    public void NotifyRoomClosed(RoomSnapshot room, int originAccountId)
    {
        var closedRoom = new NotifiedRoom(room, originAccountId);
        ClosedRooms.Add(closedRoom);
    }
}
