using System;
using System.Collections.Generic;
using Bastion.Domain.Common;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomRegistry : IRoomRegistry
{
    private readonly object _roomsLock;
    private readonly Dictionary<string, WaitingRoom> _roomsByCode;
    private readonly Dictionary<int, WaitingRoom> _roomsByAccountId;
    private readonly Dictionary<int, FailedCodeAttemptHistory> _attemptHistoriesByAccountId;
    private readonly IRoomCodeGenerator _codeGenerator;

    public RoomRegistry(IRoomCodeGenerator codeGenerator)
    {
        ArgumentNullException.ThrowIfNull(codeGenerator);

        _roomsLock = new object();
        _roomsByCode = [];
        _roomsByAccountId = [];
        _attemptHistoriesByAccountId = [];
        _codeGenerator = codeGenerator;
    }

    public bool IsInRoom(int accountId)
    {
        lock (_roomsLock)
        {
            return _roomsByAccountId.ContainsKey(accountId);
        }
    }

    public string CreateUnusedCode()
    {
        lock (_roomsLock)
        {
            string createdCode = _codeGenerator.CreateCode();
            if (_roomsByCode.ContainsKey(createdCode))
            {
                return string.Empty;
            }

            return createdCode;
        }
    }

    public bool TryOpen(WaitingRoom room)
    {
        ArgumentNullException.ThrowIfNull(room);

        lock (_roomsLock)
        {
            if (_roomsByAccountId.ContainsKey(room.HostAccountId) || _roomsByCode.ContainsKey(room.Identity.Code))
            {
                return false;
            }

            _roomsByCode.Add(room.Identity.Code, room);
            _roomsByAccountId.Add(room.HostAccountId, room);
            return true;
        }
    }

    public RoomSnapshot Admit(string code, RoomMember member, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(member);

        lock (_roomsLock)
        {
            WaitingRoom room = ClaimRoomToJoin(code, member.AccountId, now);
            room.Admit(member);
            _roomsByAccountId.Add(member.AccountId, room);
            return RoomSnapshot.Capture(room);
        }
    }

    public RoomSnapshot SetReady(int accountId, bool isReady)
    {
        lock (_roomsLock)
        {
            WaitingRoom room = GetRoomOf(accountId);
            room.SetReady(accountId, isReady);
            return RoomSnapshot.Capture(room);
        }
    }

    public RoomSnapshot FindHostedRoom(int accountId)
    {
        lock (_roomsLock)
        {
            WaitingRoom room = GetHostedRoom(accountId);
            return RoomSnapshot.Capture(room);
        }
    }

    public RoomSnapshot ChangeSettings(int accountId, RoomSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_roomsLock)
        {
            WaitingRoom room = GetHostedRoom(accountId);
            if (!room.CanSeatAllMembers(settings))
            {
                throw new RoomRejectedException(RoomRejection.TooManyPlayers);
            }

            room.ChangeSettings(settings);
            return RoomSnapshot.Capture(room);
        }
    }

    public RoomKick Kick(int hostAccountId, string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        lock (_roomsLock)
        {
            WaitingRoom room = GetHostedRoom(hostAccountId);
            Lookup<RoomMember> foundMember = room.FindMember(nickname);
            if (!foundMember.IsFound)
            {
                return new RoomKick
                {
                    Room = RoomSnapshot.Capture(room),
                    IsMemberKicked = false,
                    KickedAccountId = 0,
                };
            }

            int kickedAccountId = foundMember.GetValue().AccountId;
            if (room.IsHost(kickedAccountId))
            {
                throw new RoomRejectedException(RoomRejection.HostNotKickable);
            }

            room.Kick(kickedAccountId);
            _roomsByAccountId.Remove(kickedAccountId);
            return new RoomKick
            {
                Room = RoomSnapshot.Capture(room),
                IsMemberKicked = true,
                KickedAccountId = kickedAccountId,
            };
        }
    }

    public Lookup<RoomDeparture> Leave(int accountId)
    {
        lock (_roomsLock)
        {
            WaitingRoom? room = _roomsByAccountId.GetValueOrDefault(accountId);
            if (room is null)
            {
                return Lookup.Missing<RoomDeparture>($"AccountId={accountId}");
            }

            if (room.IsHost(accountId))
            {
                RemoveRoom(room);
                var closedRoom = RoomSnapshot.Capture(room);
                var hostDeparture = new RoomDeparture(closedRoom, isRoomClosed: true);
                return Lookup.Found(hostDeparture);
            }

            room.Remove(accountId);
            _roomsByAccountId.Remove(accountId);
            var remainingRoom = RoomSnapshot.Capture(room);
            var memberDeparture = new RoomDeparture(remainingRoom, isRoomClosed: false);
            return Lookup.Found(memberDeparture);
        }
    }

    public Lookup<RoomSnapshot> FindRoomOf(int accountId)
    {
        lock (_roomsLock)
        {
            WaitingRoom? room = _roomsByAccountId.GetValueOrDefault(accountId);
            if (room is null)
            {
                return Lookup.Missing<RoomSnapshot>($"AccountId={accountId}");
            }

            var ownRoom = RoomSnapshot.Capture(room);
            return Lookup.Found(ownRoom);
        }
    }

    public TResult StartMatch<TResult>(int hostAccountId, Func<RoomSnapshot, TResult> matchStarter)
    {
        ArgumentNullException.ThrowIfNull(matchStarter);

        lock (_roomsLock)
        {
            WaitingRoom room = GetHostedRoom(hostAccountId);
            if (!room.IsReadyToStart())
            {
                throw new RoomRejectedException(RoomRejection.NotReadyToStart);
            }

            var capturedRoom = RoomSnapshot.Capture(room);
            TResult result = matchStarter(capturedRoom);
            RemoveRoom(room);
            return result;
        }
    }

    private WaitingRoom ClaimRoomToJoin(string code, int accountId, DateTime now)
    {
        if (_roomsByAccountId.ContainsKey(accountId))
        {
            throw new RoomRejectedException(RoomRejection.InRoom);
        }

        FailedCodeAttemptHistory attemptHistory = GetAttemptHistory(accountId);
        if (attemptHistory.IsLimitReached(now))
        {
            throw new RoomRejectedException(RoomRejection.TooManyAttempts, attemptHistory.GetRetryDelay(now));
        }

        WaitingRoom? room = _roomsByCode.GetValueOrDefault(code);
        if (room is null)
        {
            attemptHistory.Record(now);
            throw new OpenRoomNotFoundException();
        }

        ForgetEmptyHistory(accountId, attemptHistory);
        if (room.HasKicked(accountId))
        {
            throw new RoomRejectedException(RoomRejection.Kicked);
        }

        if (room.IsFull())
        {
            throw new RoomRejectedException(RoomRejection.RoomFull);
        }

        return room;
    }

    private FailedCodeAttemptHistory GetAttemptHistory(int accountId)
    {
        FailedCodeAttemptHistory? attemptHistory = _attemptHistoriesByAccountId.GetValueOrDefault(accountId);
        if (attemptHistory is null)
        {
            attemptHistory = new FailedCodeAttemptHistory();
            _attemptHistoriesByAccountId.Add(accountId, attemptHistory);
        }

        return attemptHistory;
    }

    private void ForgetEmptyHistory(int accountId, FailedCodeAttemptHistory attemptHistory)
    {
        if (!attemptHistory.HasAttempts())
        {
            _attemptHistoriesByAccountId.Remove(accountId);
        }
    }

    private WaitingRoom GetHostedRoom(int accountId)
    {
        WaitingRoom room = GetRoomOf(accountId);
        if (!room.IsHost(accountId))
        {
            throw new RoomRejectedException(RoomRejection.NotHost);
        }

        return room;
    }

    private WaitingRoom GetRoomOf(int accountId)
    {
        WaitingRoom? room = _roomsByAccountId.GetValueOrDefault(accountId);
        if (room is null)
        {
            throw new RoomRejectedException(RoomRejection.NotInRoom);
        }

        return room;
    }

    private void RemoveRoom(WaitingRoom room)
    {
        _roomsByCode.Remove(room.Identity.Code);
        foreach (RoomMember member in room.Members)
        {
            _roomsByAccountId.Remove(member.AccountId);
        }
    }
}
