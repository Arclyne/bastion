using System;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

public interface IRoomKeeper
{
    RoomSnapshot Open(ConnectedPlayer host, RoomSettings settings, DateTime now);

    RoomSnapshot Join(ConnectedPlayer player, string code, DateTime now);

    RoomSnapshot SetReady(int accountId, bool isReady);

    RoomSnapshot ChangeSettings(int accountId, RoomSettings settings, DateTime now);

    void Leave(int accountId, DateTime now);

    RoomSnapshot? FindRoomOf(int accountId);

    int CloseRoomsLeftOpen(DateTime now);
}
