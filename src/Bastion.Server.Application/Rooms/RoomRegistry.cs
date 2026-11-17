using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        _roomsByCode = new Dictionary<string, WaitingRoom>();
        _roomsByAccountId = new Dictionary<int, WaitingRoom>();
        _attemptHistoriesByAccountId = new Dictionary<int, FailedCodeAttemptHistory>();
        _codeGenerator = codeGenerator;
    }

    public bool IsInRoom(int accountId)
    {
        lock (_roomsLock)
        {
            return _roomsByAccountId.ContainsKey(accountId);
        }
    }

    public bool TryCreateUnusedCode(
        [NotNullWhen(true)]
        out string? code)
    {
        lock (_roomsLock)
        {
            string createdCode = _codeGenerator.CreateCode();
            if (_roomsByCode.ContainsKey(createdCode))
            {
                code = null;
                return false;
            }

            code = createdCode;
            return true;
        }
    }

    public bool TryOpen(
        WaitingRoom room,
        [NotNullWhen(true)]
        out RoomSnapshot? openedRoom)
    {
        ArgumentNullException.ThrowIfNull(room);

        lock (_roomsLock)
        {
            if (_roomsByAccountId.ContainsKey(room.HostAccountId) || _roomsByCode.ContainsKey(room.Identity.Code))
            {
                openedRoom = null;
                return false;
            }

            _roomsByCode.Add(room.Identity.Code, room);
            _roomsByAccountId.Add(room.HostAccountId, room);
            openedRoom = RoomSnapshot.Capture(room);
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

    public RoomDeparture? Leave(int accountId)
    {
        lock (_roomsLock)
        {
            if (!_roomsByAccountId.TryGetValue(accountId, out WaitingRoom? room))
            {
                return null;
            }

            if (room.IsHost(accountId))
            {
                RemoveRoom(room);
                var closedRoom = RoomSnapshot.Capture(room);
                return new RoomDeparture(closedRoom, isRoomClosed: true);
            }

            room.Remove(accountId);
            _roomsByAccountId.Remove(accountId);
            var remainingRoom = RoomSnapshot.Capture(room);
            return new RoomDeparture(remainingRoom, isRoomClosed: false);
        }
    }

    public RoomSnapshot? FindRoomOf(int accountId)
    {
        lock (_roomsLock)
        {
            if (!_roomsByAccountId.TryGetValue(accountId, out WaitingRoom? room))
            {
                return null;
            }

            return RoomSnapshot.Capture(room);
        }
    }

    public TResult StartMatch<TResult>(int hostAccountId, Func<RoomSnapshot, TResult> startMatch)
    {
        ArgumentNullException.ThrowIfNull(startMatch);

        lock (_roomsLock)
        {
            WaitingRoom room = GetHostedRoom(hostAccountId);
            if (!room.IsReadyToStart())
            {
                throw new RoomRejectedException(RoomRejection.NotReadyToStart);
            }

            var capturedRoom = RoomSnapshot.Capture(room);
            TResult result = startMatch(capturedRoom);
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

        if (!_roomsByCode.TryGetValue(code, out WaitingRoom? room))
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
        if (!_attemptHistoriesByAccountId.TryGetValue(accountId, out FailedCodeAttemptHistory? attemptHistory))
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
        if (!_roomsByAccountId.TryGetValue(accountId, out WaitingRoom? room))
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
