using System;
using Bastion.Domain.Common;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomKeeper : IRoomKeeper
{
    public const int MaximumCodeDraws = 10;

    private readonly IRoomRegistry _roomRegistry;
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomNotifier _roomNotifier;

    public RoomKeeper(IRoomRegistry roomRegistry, IRoomRepository roomRepository, IRoomNotifier roomNotifier)
    {
        ArgumentNullException.ThrowIfNull(roomRegistry);
        ArgumentNullException.ThrowIfNull(roomRepository);
        ArgumentNullException.ThrowIfNull(roomNotifier);

        _roomRegistry = roomRegistry;
        _roomRepository = roomRepository;
        _roomNotifier = roomNotifier;
    }

    public RoomSnapshot Open(ConnectedPlayer host, RoomSettings settings, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(settings);

        string code = CreateFreeCode();
        var opening = new RoomOpening(code, host.AccountId, now);
        var storedRoom = Room.Open(opening, settings);
        int roomId = _roomRepository.AddRoom(storedRoom);
        var identity = new RoomIdentity(roomId, code);
        var hostMember = new RoomMember(host.AccountId, host.Nickname);
        var room = new WaitingRoom(identity, settings, hostMember);
        if (!_roomRegistry.TryOpen(room, out RoomSnapshot? openedRoom))
        {
            _roomRepository.CloseRoom(roomId, now);
            throw new RoomRejectedException(RoomRejection.InRoom);
        }

        return openedRoom;
    }

    public RoomSnapshot Join(ConnectedPlayer player, string code, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(code);

        var member = new RoomMember(player.AccountId, player.Nickname);
        RoomSnapshot room = AdmitToOpenRoom(member, code, now);
        _roomNotifier.NotifyRoomUpdated(room, player.AccountId);
        return room;
    }

    public RoomSnapshot SetReady(int accountId, bool isReady)
    {
        RoomSnapshot room = _roomRegistry.SetReady(accountId, isReady);
        _roomNotifier.NotifyRoomUpdated(room, accountId);
        return room;
    }

    public RoomSnapshot ChangeSettings(int accountId, RoomSettings settings, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(settings);

        RoomSnapshot hostedRoom = _roomRegistry.FindHostedRoom(accountId);
        if (!hostedRoom.CanSeatAllMembers(settings))
        {
            throw new RoomRejectedException(RoomRejection.TooManyPlayers);
        }

        _roomRepository.ChangeRoomSettings(hostedRoom.Identity.RoomId, settings, now);
        RoomSnapshot room = _roomRegistry.ChangeSettings(accountId, settings);
        _roomNotifier.NotifyRoomUpdated(room, accountId);
        return room;
    }

    public void Leave(int accountId, DateTime now)
    {
        Lookup<RoomDeparture> foundDeparture = _roomRegistry.Leave(accountId);
        if (!foundDeparture.IsFound)
        {
            return;
        }

        RoomDeparture departure = foundDeparture.GetValue();
        if (!departure.IsRoomClosed)
        {
            _roomNotifier.NotifyRoomUpdated(departure.Room, accountId);
            return;
        }

        _roomNotifier.NotifyRoomClosed(departure.Room, accountId);
        _roomRepository.CloseRoom(departure.Room.Identity.RoomId, now);
    }

    public Lookup<RoomSnapshot> FindRoomOf(int accountId)
    {
        return _roomRegistry.FindRoomOf(accountId);
    }

    public int CloseRoomsLeftOpen(DateTime now)
    {
        return _roomRepository.CloseOpenRooms(now);
    }

    private string CreateFreeCode()
    {
        for (int draw = 0; draw < MaximumCodeDraws; draw++)
        {
            if (_roomRegistry.TryCreateUnusedCode(out string? code) && !_roomRepository.IsCodeTaken(code))
            {
                return code;
            }
        }

        throw new RoomCodeUnavailableException();
    }

    private RoomSnapshot AdmitToOpenRoom(RoomMember member, string code, DateTime now)
    {
        try
        {
            return _roomRegistry.Admit(code, member, now);
        }
        catch (OpenRoomNotFoundException ex)
        {
            RoomRejection rejection = GetMissingRoomRejection(code);
            throw new RoomRejectedException(rejection, ex);
        }
    }

    private RoomRejection GetMissingRoomRejection(string code)
    {
        if (_roomRepository.IsRoomInMatch(code))
        {
            return RoomRejection.MatchStarted;
        }

        return RoomRejection.RoomNotFound;
    }
}
