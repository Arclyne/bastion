using System.Collections.Generic;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomNotifier : IRoomNotifier
{
    public FakeRoomNotifier()
    {
        UpdatedRooms = [];
        ClosedRooms = [];
        KickedAccountIds = [];
    }

    public List<NotifiedRoom> UpdatedRooms { get; }

    public List<NotifiedRoom> ClosedRooms { get; }

    public List<int> KickedAccountIds { get; }

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

    public void NotifyKicked(RoomSnapshot room, int kickedAccountId)
    {
        KickedAccountIds.Add(kickedAccountId);
    }
}
