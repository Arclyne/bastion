using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Common;

namespace Bastion.Domain.Rooms;

public sealed class WaitingRoom
{
    private const int FirstSeatIndex = 0;

    private readonly List<RoomMember> _members;
    private readonly HashSet<int> _kickedAccountIds;

    public WaitingRoom(RoomIdentity identity, RoomSettings settings, RoomMember host)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(host);

        _members = [];
        _kickedAccountIds = [];
        Identity = identity;
        Settings = settings;
        HostAccountId = host.AccountId;
        host.TakeSeat(FirstSeatIndex);
        host.SetReady(isReady: true);
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
        member.SetReady(isReady: false);
        _members.Add(member);
        _members.Sort(CompareSeats);
    }

    public void SetReady(int accountId, bool isReady)
    {
        if (IsHost(accountId))
        {
            return;
        }

        RoomMember member = GetMember(accountId);
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

    public Lookup<RoomMember> FindMember(string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        RoomMember? member = _members.FirstOrDefault(candidate => candidate.Nickname == nickname);
        if (member is null)
        {
            return Lookup.Missing<RoomMember>($"Nickname={nickname}");
        }

        return Lookup.Found(member);
    }

    public void Kick(int accountId)
    {
        if (IsHost(accountId))
        {
            throw new InvalidOperationException("The host cannot be kicked from their own room.");
        }

        _kickedAccountIds.Add(accountId);
        RoomMember? member = _members.FirstOrDefault(candidate => candidate.AccountId == accountId);
        if (member is not null)
        {
            _members.Remove(member);
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

    private int GetSeatCount()
    {
        return Settings.GameMode.PlayerCount;
    }

    private bool HasMember(int accountId)
    {
        return _members.Any(member => member.AccountId == accountId);
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

    private static int CompareSeats(RoomMember first, RoomMember second)
    {
        return first.SeatIndex.CompareTo(second.SeatIndex);
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
