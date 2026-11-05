using System;
using System.Linq;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Rooms;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;
using Xunit;

namespace Bastion.Client.Tests.Rooms;

public class TestRoomNotifications
{
    private const string Code = "K7MTQ3";
    private const string HostNickname = "marta_muros";
    private const string GuestNickname = "pedro_peon";
    private const byte WallsPerPlayer = 6;
    private const byte ClockMinutes = 3;
    private const int TwoSeats = 2;
    private const int FirstSeatIndex = 0;
    private const int SecondSeatIndex = 1;

    private static readonly DateTime _occurredAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    private readonly NotificationCallback _notificationCallback = new NotificationCallback();
    private RoomUpdatedEventArgs? _roomUpdated;
    private RoomClosedEventArgs? _roomClosed;

    public TestRoomNotifications()
    {
        _notificationCallback.RoomUpdated += HandleRoomUpdated;
        _notificationCallback.RoomClosed += HandleRoomClosed;
    }

    [Fact]
    public void ReceiveNotification_RoomUpdated_RaisesMembersInSeatOrder()
    {
        string[] expectedNicknames = new string[] { HostNickname, GuestNickname };

        _notificationCallback.ReceiveNotification(CreateRoomUpdatedNotification());

        Assert.Equal(expectedNicknames, _roomUpdated?.Room.Members.Select(member => member.Nickname));
    }

    [Fact]
    public void ReceiveNotification_RoomUpdated_RaisesRoomOptions()
    {
        var expectedOptions = new PrivateRoomOptions(PrivateRoomGameMode.Quick, WallsPerPlayer, ClockMinutes);

        _notificationCallback.ReceiveNotification(CreateRoomUpdatedNotification());

        Assert.Equal(expectedOptions, _roomUpdated?.Room.Options);
    }

    [Fact]
    public void ReceiveNotification_RoomClosed_RaisesClosedCode()
    {
        var notification = new RoomClosedNotification
        {
            OccurredAt = _occurredAt,
            Code = Code,
            Reason = RoomClosedReason.HostLeft,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(Code, _roomClosed?.Code);
    }

    private static RoomUpdatedNotification CreateRoomUpdatedNotification()
    {
        var guest = new RoomMemberState
        {
            Nickname = GuestNickname,
            SeatIndex = SecondSeatIndex,
        };
        var host = new RoomMemberState
        {
            Nickname = HostNickname,
            SeatIndex = FirstSeatIndex,
            IsReady = true,
            IsHost = true,
        };
        var room = new RoomState
        {
            Code = Code,
            SeatCount = TwoSeats,
            Options = new RoomOptions
            {
                GameMode = RoomGameMode.Quick,
                WallsPerPlayer = WallsPerPlayer,
                ClockMinutes = ClockMinutes,
            },
            Members = new RoomMemberState[] { guest, host },
        };
        return new RoomUpdatedNotification
        {
            OccurredAt = _occurredAt,
            Room = room,
        };
    }

    private void HandleRoomUpdated(object? sender, RoomUpdatedEventArgs e)
    {
        _roomUpdated = e;
    }

    private void HandleRoomClosed(object? sender, RoomClosedEventArgs e)
    {
        _roomClosed = e;
    }
}
