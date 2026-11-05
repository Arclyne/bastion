using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public sealed record RoomSnapshot(RoomIdentity Identity, RoomSettings Settings, IReadOnlyList<RoomMemberSnapshot> Members)
{
    public int SeatCount => Settings.GameMode.PlayerCount;

    public static RoomSnapshot Capture(WaitingRoom room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var members = room.Members
            .Select(member => CaptureMember(room, member))
            .ToList();
        return new RoomSnapshot(room.Identity, room.Settings, members);
    }

    public IReadOnlyList<int> ListAccountIdsExcept(int excludedAccountId)
    {
        return Members
            .Select(member => member.AccountId)
            .Where(accountId => accountId != excludedAccountId)
            .ToList();
    }

    private static RoomMemberSnapshot CaptureMember(WaitingRoom room, RoomMember member)
    {
        return new RoomMemberSnapshot(
            member.AccountId,
            member.Nickname,
            member.SeatIndex,
            member.IsReady,
            room.IsHost(member.AccountId));
    }
}
