using System;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public class TestRoom
{
    private const string InvalidCode = "K7MTO3";
    private const byte ChangedWalls = 4;

    private static readonly DateTime _openedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _changedAt = _openedAt.AddMinutes(2);

    [Fact]
    public void Open_InvalidCode_ThrowsArgument()
    {
        var opening = new RoomOpening(InvalidCode, RoomSamples.HostAccountId, _openedAt);

        Assert.Throws<ArgumentException>(() => Room.Open(opening, RoomSamples.CreateTwoPlayerSettings()));
    }

    [Fact]
    public void Open_ValidCode_StartsOpen()
    {
        Room room = OpenRoom();

        Assert.Equal(RoomStatus.Open, room.Status);
    }

    [Fact]
    public void ChangeSettings_OpenRoom_RecordsActivity()
    {
        Room room = OpenRoom();
        RoomSettings settings = RoomSamples.CreateTwoPlayerSettings() with { WallsPerPlayer = ChangedWalls };

        room.ChangeSettings(settings, _changedAt);

        Assert.Equal((ChangedWalls, _changedAt), (room.WallsPerPlayer, room.LastActivityAt));
    }

    [Fact]
    public void Close_OpenRoom_MarksClosed()
    {
        Room room = OpenRoom();

        room.Close(_changedAt);

        Assert.Equal(RoomStatus.Closed, room.Status);
    }

    [Fact]
    public void ChangeSettings_ClosedRoom_ThrowsInvalidOperation()
    {
        Room room = OpenRoom();
        room.Close(_changedAt);

        Assert.Throws<InvalidOperationException>(
            () => room.ChangeSettings(RoomSamples.CreateTwoPlayerSettings(), _changedAt));
    }

    private static Room OpenRoom()
    {
        var opening = new RoomOpening(RoomSamples.Code, RoomSamples.HostAccountId, _openedAt);
        return Room.Open(opening, RoomSamples.CreateTwoPlayerSettings());
    }
}
