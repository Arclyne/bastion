using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomRegistry : IRoomRegistry
{
    private readonly object _roomsLock = new object();
    private readonly Dictionary<string, WaitingRoom> _roomsByCode = new Dictionary<string, WaitingRoom>();
    private readonly Dictionary<int, WaitingRoom> _roomsByAccountId = new Dictionary<int, WaitingRoom>();
    private readonly Dictionary<int, FailedCodeAttempts> _failedAttemptsByAccountId =
        new Dictionary<int, FailedCodeAttempts>();
    private readonly IRoomCodeGenerator _codeGenerator;

    public RoomRegistry(IRoomCodeGenerator codeGenerator)
    {
        ArgumentNullException.ThrowIfNull(codeGenerator);

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
            string code = _codeGenerator.CreateCode();
            while (_roomsByCode.ContainsKey(code))
            {
                code = _codeGenerator.CreateCode();
            }

            return code;
        }
    }

    public RoomSnapshot? TryOpen(WaitingRoom room)
    {
        ArgumentNullException.ThrowIfNull(room);

        lock (_roomsLock)
        {
            if (_roomsByAccountId.ContainsKey(room.HostAccountId) || _roomsByCode.ContainsKey(room.Identity.Code))
            {
                return null;
            }

            _roomsByCode.Add(room.Identity.Code, room);
            _roomsByAccountId.Add(room.HostAccountId, room);
            return RoomSnapshot.Capture(room);
        }
    }

    public RoomSnapshot Admit(string code, RoomMember member, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(member);

        lock (_roomsLock)
        {
            WaitingRoom room = FindRoomToJoin(code, member.AccountId, now);
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
            return RoomSnapshot.Capture(GetHostedRoom(accountId));
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
                return new RoomDeparture(RoomSnapshot.Capture(room), accountId, true);
            }

            room.Remove(accountId);
            _roomsByAccountId.Remove(accountId);
            return new RoomDeparture(RoomSnapshot.Capture(room), accountId, false);
        }
    }

    public RoomSnapshot? FindRoomOf(int accountId)
    {
        lock (_roomsLock)
        {
            return _roomsByAccountId.TryGetValue(accountId, out WaitingRoom? room) ? RoomSnapshot.Capture(room) : null;
        }
    }

    private WaitingRoom FindRoomToJoin(string code, int accountId, DateTime now)
    {
        if (_roomsByAccountId.ContainsKey(accountId))
        {
            throw new RoomRejectedException(RoomRejection.InRoom);
        }

        FailedCodeAttempts failedAttempts = GetFailedAttempts(accountId);
        if (failedAttempts.IsLimitReached(now))
        {
            throw new RoomRejectedException(RoomRejection.TooManyAttempts, failedAttempts.GetRetryDelay(now));
        }

        if (!_roomsByCode.TryGetValue(code, out WaitingRoom? room))
        {
            failedAttempts.Record(now);
            throw new RoomRejectedException(RoomRejection.RoomNotFound);
        }

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

    private FailedCodeAttempts GetFailedAttempts(int accountId)
    {
        if (!_failedAttemptsByAccountId.TryGetValue(accountId, out FailedCodeAttempts? failedAttempts))
        {
            failedAttempts = new FailedCodeAttempts();
            _failedAttemptsByAccountId.Add(accountId, failedAttempts);
        }

        return failedAttempts;
    }

    private WaitingRoom GetRoomOf(int accountId)
    {
        if (!_roomsByAccountId.TryGetValue(accountId, out WaitingRoom? room))
        {
            throw new RoomRejectedException(RoomRejection.NotInRoom);
        }

        return room;
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

    private void RemoveRoom(WaitingRoom room)
    {
        _roomsByCode.Remove(room.Identity.Code);
        foreach (int memberAccountId in room.Members.Select(member => member.AccountId))
        {
            _roomsByAccountId.Remove(memberAccountId);
        }
    }
}
