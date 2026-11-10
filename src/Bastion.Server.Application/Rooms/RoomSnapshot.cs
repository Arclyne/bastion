using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public sealed record RoomSnapshot
{
    public RoomSnapshot(RoomIdentity identity, RoomSettings settings, IReadOnlyList<RoomMemberSnapshot> members)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(members);

        Identity = identity;
        Settings = settings;
        Members = members;
    }

    public RoomIdentity Identity { get; }

    public RoomSettings Settings { get; }

    public IReadOnlyList<RoomMemberSnapshot> Members { get; }

    public static RoomSnapshot Capture(WaitingRoom room)
    {
        ArgumentNullException.ThrowIfNull(room);

        IEnumerable<RoomMemberSnapshot> members = room.Members.Select(member => CaptureMember(room, member));
        var memberList = new List<RoomMemberSnapshot>(members);
        return new RoomSnapshot(room.Identity, room.Settings, memberList);
    }

    public int GetSeatCount()
    {
        return Settings.GameMode.PlayerCount;
    }

    public bool CanSeatAllMembers(RoomSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return Members.Count <= settings.GameMode.PlayerCount;
    }

    public IReadOnlyList<int> ListAccountIdsExcept(int excludedAccountId)
    {
        IEnumerable<int> accountIds = Members
            .Select(member => member.AccountId)
            .Where(accountId => accountId != excludedAccountId);
        return new List<int>(accountIds);
    }

    private static RoomMemberSnapshot CaptureMember(WaitingRoom room, RoomMember member)
    {
        bool isHost = room.IsHost(member.AccountId);
        return new RoomMemberSnapshot
        {
            AccountId = member.AccountId,
            Nickname = member.Nickname,
            SeatIndex = member.SeatIndex,
            IsReady = member.IsReady,
            IsHost = isHost,
        };
    }
}
