using Bastion.Client.Networking.Rooms;
using Bastion.Client.Screens.MainMenu.Rooms;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu.Rooms;

public sealed class TestRoomSettingsSheetViewModel
{
    private const byte QuickWalls = 6;
    private const int ThreeMembers = 3;
    private const int ClassicPosition = 0;
    private const int FourPlayersPosition = 1;

    private readonly RoomSettingsSheetViewModel _sheet = new RoomSettingsSheetViewModel();

    [Fact]
    public void OpenToCreate_FirstTime_ProposesTheClassicWalls()
    {
        _sheet.OpenToCreate();

        Assert.Equal(PrivateRoomSampleFactory.ClassicWalls, _sheet.Walls);
    }

    [Fact]
    public void SelectMode_Quick_ProposesTheQuickWalls()
    {
        _sheet.OpenToCreate();

        _sheet.SelectMode(PrivateRoomGameMode.Quick);

        Assert.Equal(QuickWalls, _sheet.Walls);
    }

    [Fact]
    public void IncreaseWalls_AtTheMaximum_KeepsTheMaximum()
    {
        var options = new PrivateRoomOptions(
            PrivateRoomGameMode.Classic,
            RoomPolicy.MaximumWallsPerPlayer,
            PrivateRoomSampleFactory.FiveMinutes);
        _sheet.OpenToEdit(options, memberCount: 1);

        _sheet.IncreaseWalls();

        Assert.Equal(RoomPolicy.MaximumWallsPerPlayer, _sheet.Walls);
    }

    [Fact]
    public void DecreaseWalls_AtTheMinimum_CannotGoLower()
    {
        var options = new PrivateRoomOptions(
            PrivateRoomGameMode.Classic,
            RoomPolicy.MinimumWallsPerPlayer,
            PrivateRoomSampleFactory.FiveMinutes);
        _sheet.OpenToEdit(options, memberCount: 1);

        bool canDecrease = _sheet.CanDecreaseWalls;

        Assert.False(canDecrease);
    }

    [Fact]
    public void OpenToEdit_ThreeMembers_TurnsOffTwoSeatModes()
    {
        var options = new PrivateRoomOptions(
            PrivateRoomGameMode.FourPlayers,
            QuickWalls,
            PrivateRoomSampleFactory.FiveMinutes);

        _sheet.OpenToEdit(options, ThreeMembers);

        Assert.False(_sheet.Modes[ClassicPosition].IsAvailable);
    }

    [Fact]
    public void SelectMode_TurnedOffMode_KeepsTheCurrentMode()
    {
        var options = new PrivateRoomOptions(
            PrivateRoomGameMode.FourPlayers,
            QuickWalls,
            PrivateRoomSampleFactory.FiveMinutes);
        _sheet.OpenToEdit(options, ThreeMembers);

        _sheet.SelectMode(PrivateRoomGameMode.Classic);

        Assert.True(_sheet.Modes[FourPlayersPosition].IsSelected);
    }

    [Fact]
    public void HasMembersNotice_EditingWithGuest_IsTrue()
    {
        _sheet.OpenToEdit(PrivateRoomSampleFactory.ClassicOptions, PrivateRoomSampleFactory.TwoSeats);

        bool hasNotice = _sheet.HasMembersNotice;

        Assert.True(hasNotice);
    }

    [Fact]
    public void GetOptions_ChosenClock_ReturnsTheClock()
    {
        _sheet.OpenToCreate();
        _sheet.SelectClock(PrivateRoomSampleFactory.ThreeMinutes);

        PrivateRoomOptions options = _sheet.GetOptions();

        Assert.Equal(PrivateRoomSampleFactory.ThreeMinutes, options.ClockMinutes);
    }
}
