using System;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomKeeper : IRoomKeeper
{
    private const int MaximumCodeAttempts = 10;

    private readonly IRoomRegistry _roomRegistry;
    private readonly IRoomStore _roomStore;
    private readonly IRoomNotifier _roomNotifier;

    public RoomKeeper(IRoomRegistry roomRegistry, IRoomStore roomStore, IRoomNotifier roomNotifier)
    {
        ArgumentNullException.ThrowIfNull(roomRegistry);
        ArgumentNullException.ThrowIfNull(roomStore);
        ArgumentNullException.ThrowIfNull(roomNotifier);

        _roomRegistry = roomRegistry;
        _roomStore = roomStore;
        _roomNotifier = roomNotifier;
    }

    public RoomSnapshot Open(ConnectedPlayer host, RoomSettings settings, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(settings);

        string code = CreateFreeCode();
        int roomId = _roomStore.AddRoom(Room.Open(new RoomOpening(code, host.AccountId, now), settings));
        var room = new WaitingRoom(
            new RoomIdentity(roomId, code),
            settings,
            new RoomMember(host.AccountId, host.Nickname));
        RoomSnapshot? openedRoom = _roomRegistry.TryOpen(room);
        if (openedRoom is null)
        {
            _roomStore.CloseRoom(roomId, now);
            throw new RoomRejectedException(RoomRejection.InRoom);
        }

        return openedRoom;
    }

    public RoomSnapshot Join(ConnectedPlayer player, string code, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(code);

        RoomSnapshot room = AdmitToOpenRoom(new RoomMember(player.AccountId, player.Nickname), code, now);
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
        if (hostedRoom.Members.Count > settings.GameMode.PlayerCount)
        {
            throw new RoomRejectedException(RoomRejection.TooManyPlayers);
        }

        _roomStore.ChangeRoomSettings(hostedRoom.Identity.RoomId, settings, now);
        RoomSnapshot room = _roomRegistry.ChangeSettings(accountId, settings);
        _roomNotifier.NotifyRoomUpdated(room, accountId);
        return room;
    }

    public void Leave(int accountId, DateTime now)
    {
        RoomDeparture? departure = _roomRegistry.Leave(accountId);
        if (departure is null)
        {
            return;
        }

        if (!departure.IsRoomClosed)
        {
            _roomNotifier.NotifyRoomUpdated(departure.Room, accountId);
            return;
        }

        _roomNotifier.NotifyRoomClosed(departure.Room, accountId);
        _roomStore.CloseRoom(departure.Room.Identity.RoomId, now);
    }

    public RoomSnapshot? FindRoomOf(int accountId)
    {
        return _roomRegistry.FindRoomOf(accountId);
    }

    public int CloseRoomsLeftOpen(DateTime now)
    {
        return _roomStore.CloseOpenRooms(now);
    }

    private RoomSnapshot AdmitToOpenRoom(RoomMember member, string code, DateTime now)
    {
        try
        {
            return _roomRegistry.Admit(code, member, now);
        }
        catch (RoomRejectedException ex) when (IsCodeOfStartedRoom(ex, code))
        {
            throw new RoomRejectedException(RoomRejection.MatchStarted);
        }
    }

    private bool IsCodeOfStartedRoom(RoomRejectedException rejection, string code)
    {
        return rejection.Rejection == RoomRejection.RoomNotFound && _roomStore.IsRoomInMatch(code);
    }

    private string CreateFreeCode()
    {
        for (int attempt = 0; attempt < MaximumCodeAttempts; attempt++)
        {
            string code = _roomRegistry.CreateUnusedCode();
            if (!_roomStore.IsCodeTaken(code))
            {
                return code;
            }
        }

        throw new InvalidOperationException("No free room code was found.");
    }
}
