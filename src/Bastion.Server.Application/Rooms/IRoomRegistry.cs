using System;
using System.Diagnostics.CodeAnalysis;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public interface IRoomRegistry
{
    bool IsInRoom(int accountId);

    string CreateUnusedCode();

    bool TryOpen(WaitingRoom room, [NotNullWhen(true)] out RoomSnapshot? openedRoom);

    RoomSnapshot Admit(string code, RoomMember member, DateTime now);

    RoomSnapshot SetReady(int accountId, bool isReady);

    RoomSnapshot FindHostedRoom(int accountId);

    RoomSnapshot ChangeSettings(int accountId, RoomSettings settings);

    RoomDeparture? Leave(int accountId);

    RoomSnapshot? FindRoomOf(int accountId);

    TResult StartMatch<TResult>(int hostAccountId, Func<RoomSnapshot, TResult> startMatch);
}
