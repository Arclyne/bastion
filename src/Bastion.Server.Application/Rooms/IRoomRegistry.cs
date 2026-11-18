using System;
using Bastion.Domain.Common;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public interface IRoomRegistry
{
    bool IsInRoom(int accountId);

    string CreateUnusedCode();

    bool TryOpen(WaitingRoom room);

    RoomSnapshot Admit(string code, RoomMember member, DateTime now);

    RoomSnapshot SetReady(int accountId, bool isReady);

    RoomSnapshot FindHostedRoom(int accountId);

    RoomSnapshot ChangeSettings(int accountId, RoomSettings settings);

    Lookup<RoomDeparture> Leave(int accountId);

    Lookup<RoomSnapshot> FindRoomOf(int accountId);

    TResult StartMatch<TResult>(int hostAccountId, Func<RoomSnapshot, TResult> matchStarter);
}
