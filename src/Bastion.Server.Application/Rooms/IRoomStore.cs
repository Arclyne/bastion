using System;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public interface IRoomStore
{
    bool IsCodeTaken(string code);

    int AddRoom(Room room);

    void ChangeRoomSettings(int roomId, RoomSettings settings, DateTime changedAt);

    void CloseRoom(int roomId, DateTime closedAt);

    int CloseOpenRooms(DateTime closedAt);
}
