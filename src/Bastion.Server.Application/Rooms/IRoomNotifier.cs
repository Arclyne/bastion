namespace Bastion.Server.Application.Rooms;

public interface IRoomNotifier
{
    void NotifyRoomUpdated(RoomSnapshot room, int originAccountId);

    void NotifyRoomClosed(RoomSnapshot room, int originAccountId);
}
