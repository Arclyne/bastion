using System;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public interface IRoomRegistry
{
    bool IsInRoom(int accountId);

    string CreateUnusedCode();

    RoomSnapshot? TryOpen(WaitingRoom room);

    RoomSnapshot Admit(string code, RoomMember member, DateTime now);

    RoomSnapshot SetReady(int accountId, bool isReady);

    RoomSnapshot FindHostedRoom(int accountId);

    RoomSnapshot ChangeSettings(int accountId, RoomSettings settings);

    RoomDeparture? Leave(int accountId);

    RoomSnapshot? FindRoomOf(int accountId);

    TResult StartMatch<TResult>(int hostAccountId, Func<RoomSnapshot, TResult> startMatch);
}
