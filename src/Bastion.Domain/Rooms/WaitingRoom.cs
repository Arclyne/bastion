using System;
using System.Collections.Generic;
using System.Linq;

namespace Bastion.Domain.Rooms;

public sealed class WaitingRoom
{
    private const int FirstSeatIndex = 0;

    private readonly List<RoomMember> _members = new List<RoomMember>();
    private readonly HashSet<int> _kickedAccountIds = new HashSet<int>();

    public WaitingRoom(RoomIdentity identity, RoomSettings settings, RoomMember host)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(host);

        Identity = identity;
        Settings = settings;
        HostAccountId = host.AccountId;
        host.TakeSeat(FirstSeatIndex);
        host.SetReady(true);
        _members.Add(host);
    }

    public RoomIdentity Identity { get; }

    public RoomSettings Settings { get; private set; }

    public int HostAccountId { get; }

    public IReadOnlyList<RoomMember> Members => _members;

    public bool IsHost(int accountId)
    {
        return accountId == HostAccountId;
    }

    private bool HasMember(int accountId)
    {
        return _members.Any(member => member.AccountId == accountId);
    }

    public bool IsFull()
    {
        return _members.Count >= GetSeatCount();
    }

    public bool HasKicked(int accountId)
    {
        return _kickedAccountIds.Contains(accountId);
    }

    public bool IsReadyToStart()
    {
        return _members.Count == GetSeatCount() && _members.All(member => member.IsReady);
    }

    public bool CanSeatAllMembers(RoomSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return _members.Count <= settings.GameMode.PlayerCount;
    }

    public void Admit(RoomMember member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (IsFull() || HasKicked(member.AccountId) || HasMember(member.AccountId))
        {
            throw new InvalidOperationException("The player cannot take a seat in this room.");
        }

        member.TakeSeat(FindFirstFreeSeat());
        member.SetReady(false);
        _members.Add(member);
        _members.Sort(CompareSeats);
    }

    public void SetReady(int accountId, bool isReady)
    {
        RoomMember member = GetMember(accountId);
        if (IsHost(accountId))
        {
            return;
        }

        member.SetReady(isReady);
    }

    public void ChangeSettings(RoomSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (!CanSeatAllMembers(settings))
        {
            throw new InvalidOperationException("The new game mode has fewer seats than members in the room.");
        }

        Settings = settings;
        for (int memberIndex = 0; memberIndex < _members.Count; memberIndex++)
        {
            RoomMember member = _members[memberIndex];
            member.TakeSeat(FirstSeatIndex + memberIndex);
            member.SetReady(IsHost(member.AccountId));
        }
    }

    public void Remove(int accountId)
    {
        if (IsHost(accountId))
        {
            throw new InvalidOperationException("The host closes the room instead of leaving it.");
        }

        _members.Remove(GetMember(accountId));
    }

    private static int CompareSeats(RoomMember first, RoomMember second)
    {
        return first.SeatIndex.CompareTo(second.SeatIndex);
    }

    private int GetSeatCount()
    {
        return Settings.GameMode.PlayerCount;
    }

    private int FindFirstFreeSeat()
    {
        int seatIndex = FirstSeatIndex;
        while (_members.Any(member => member.SeatIndex == seatIndex))
        {
            seatIndex++;
        }

        return seatIndex;
    }

    private RoomMember GetMember(int accountId)
    {
        RoomMember? member = _members.FirstOrDefault(candidate => candidate.AccountId == accountId);
        if (member is null)
        {
            throw new InvalidOperationException("The player is not a member of this room.");
        }

        return member;
    }
}
