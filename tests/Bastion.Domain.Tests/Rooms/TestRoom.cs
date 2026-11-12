using System;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public sealed class TestRoom
{
    private const string InvalidCode = "K7MTO3";
    private const byte ChangedWalls = 4;
    private const int MatchId = 41;

    private static readonly DateTime _openedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _changedAt = _openedAt.AddMinutes(2);

    [Fact]
    public void Open_InvalidCode_ThrowsArgument()
    {
        var opening = new RoomOpening(InvalidCode, RoomSampleFactory.HostAccountId, _openedAt);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        Action open = () => Room.Open(opening, settings);

        Assert.Throws<ArgumentException>(open);
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
        RoomSettings twoPlayerSettings = RoomSampleFactory.CreateTwoPlayerSettings();
        RoomSettings settings = twoPlayerSettings with { WallsPerPlayer = ChangedWalls };
        (byte WallsPerPlayer, DateTime LastActivityAt) expectedActivity = (ChangedWalls, _changedAt);

        room.ChangeSettings(settings, _changedAt);

        Assert.Equal(expectedActivity, (room.WallsPerPlayer, room.LastActivityAt));
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
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        Action changeSettings = () => room.ChangeSettings(settings, _changedAt);

        Assert.Throws<InvalidOperationException>(changeSettings);
    }

    [Fact]
    public void StartMatch_OpenRoom_KeepsMatchInRoom()
    {
        Room room = OpenRoom();
        (RoomStatus Status, int? MatchId) expectedState = (RoomStatus.InMatch, MatchId);

        room.StartMatch(MatchId, _changedAt);

        Assert.Equal(expectedState, (room.Status, room.MatchId));
    }

    [Fact]
    public void FinishMatch_RoomInMatch_ClosesRoom()
    {
        Room room = OpenRoom();
        room.StartMatch(MatchId, _changedAt);

        room.FinishMatch(_changedAt);

        Assert.Equal(RoomStatus.Closed, room.Status);
    }

    [Fact]
    public void FinishMatch_OpenRoom_ThrowsInvalidOperation()
    {
        Room room = OpenRoom();

        Action finishMatch = () => room.FinishMatch(_changedAt);

        Assert.Throws<InvalidOperationException>(finishMatch);
    }

    private static Room OpenRoom()
    {
        var opening = new RoomOpening(RoomSampleFactory.Code, RoomSampleFactory.HostAccountId, _openedAt);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();
        return Room.Open(opening, settings);
    }
}
