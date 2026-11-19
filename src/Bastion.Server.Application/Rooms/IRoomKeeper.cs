using System;
using Bastion.Domain.Common;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

public interface IRoomKeeper
{
    RoomSnapshot Open(ConnectedPlayer host, RoomSettings settings, DateTime now);

    RoomSnapshot Join(ConnectedPlayer player, string code, DateTime now);

    RoomSnapshot SetReady(int accountId, bool isReady);

    RoomSnapshot ChangeSettings(int accountId, RoomSettings settings, DateTime now);

    RoomSnapshot Kick(int hostAccountId, string nickname);

    void Leave(int accountId, DateTime now);

    Lookup<RoomSnapshot> FindRoomOf(int accountId);

    int CloseRoomsLeftOpen(DateTime now);
}
