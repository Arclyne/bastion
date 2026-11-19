using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Rooms;
using Bastion.Client.Screens.MainMenu.Rooms;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu.Rooms;

public sealed class TestPrivateRoomViewModel : IDisposable
{
    private const string SessionToken = "session-token";
    private const string PreferredLanguage = "es-MX";
    private const int GuestSeatIndex = 1;
    private const int HostSeatIndex = 0;
    private const string TypedCode = "k7m-tq3";

    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 19, 18, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _copiedTime = TimeSpan.FromSeconds(PrivateRoomViewModel.CopiedSeconds);

    private readonly FakeRoomClient _roomClient = new FakeRoomClient();
    private readonly FakeClipboard _clipboard = new FakeClipboard();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_now);
    private PrivateRoomViewModel? _viewModel;

    public void Dispose()
    {
        _viewModel?.Dispose();
    }

    [Fact]
    public async Task ConfirmSheet_NewRoom_ShowsTheRoom()
    {
        PrivateRoomViewModel host = CreateViewModel(PrivateRoomSampleFactory.HostNickname);
        _roomClient.NextRoom = PrivateRoomSampleFactory.CreateHostAlone();
        host.OpenCreateSheet();

        await host.ConfirmSheet();

        Assert.True(host.IsInRoom);
    }

    [Fact]
    public async Task ConfirmSheet_NewRoom_SendsTheChosenOptions()
    {
        PrivateRoomViewModel host = CreateViewModel(PrivateRoomSampleFactory.HostNickname);
        _roomClient.NextRoom = PrivateRoomSampleFactory.CreateHostAlone();
        host.OpenCreateSheet();
        host.Sheet.SelectClock(PrivateRoomSampleFactory.ThreeMinutes);

        await host.ConfirmSheet();

        Assert.Equal(PrivateRoomSampleFactory.ThreeMinutes, _roomClient.CreatedOptions?.ClockMinutes);
    }

    [Fact]
    public async Task ConfirmSheet_NewRoom_ClosesTheSheet()
    {
        PrivateRoomViewModel host = CreateViewModel(PrivateRoomSampleFactory.HostNickname);
        _roomClient.NextRoom = PrivateRoomSampleFactory.CreateHostAlone();
        host.OpenCreateSheet();

        await host.ConfirmSheet();

        Assert.False(host.Sheet.IsOpen);
    }

    [Fact]
    public async Task ConfirmSheet_PlayerSearching_ShowsTheReasonInTheSheet()
    {
        PrivateRoomViewModel host = CreateViewModel(PrivateRoomSampleFactory.HostNickname);
        _roomClient.Failure = CreateRejection(RoomRejection.SearchingMatch);
        host.OpenCreateSheet();

        await host.ConfirmSheet();

        Assert.Equal(Strings.RoomBusySearching, host.Sheet.ErrorText);
    }

    [Fact]
    public async Task ConfirmSheet_ServerDown_KeepsTheMenu()
    {
        PrivateRoomViewModel host = CreateViewModel(PrivateRoomSampleFactory.HostNickname);
        _roomClient.Failure = new ServerUnreachableException();
        host.OpenCreateSheet();

        await host.ConfirmSheet();

        Assert.False(host.IsInRoom);
    }

    [Fact]
    public async Task ConfirmSheet_HostAlone_SaysOneSeatIsFree()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateHostAlone());

        string footerText = host.FooterText;

        Assert.Equal(Strings.RoomWaitingOneSeat, footerText);
    }

    [Fact]
    public async Task CanStart_GuestNotReady_IsFalse()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false));

        bool canStart = host.CanStart;

        Assert.False(canStart);
    }

    [Fact]
    public async Task CanStart_EverySeatReady_IsTrue()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: true));

        bool canStart = host.CanStart;

        Assert.True(canStart);
    }

    [Fact]
    public async Task RoomUpdated_GuestBecomesReady_EnablesStart()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false));

        _roomClient.RaiseRoomUpdated(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: true));

        Assert.True(host.CanStart);
    }

    [Fact]
    public async Task Seats_HostView_LetsTheHostKickTheGuest()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false));

        RoomSeatViewModel guestSeat = host.Seats[GuestSeatIndex];

        Assert.True(guestSeat.IsKickable);
    }

    [Fact]
    public async Task Seats_HostView_NeverOffersToKickTheHost()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false));

        RoomSeatViewModel hostSeat = host.Seats[HostSeatIndex];

        Assert.False(hostSeat.IsKickable);
    }

    [Fact]
    public async Task ConfirmKick_AskedForGuest_KicksTheGuest()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false));
        host.AskToKick(PrivateRoomSampleFactory.GuestNickname);
        _roomClient.NextRoom = PrivateRoomSampleFactory.CreateHostAlone();
        string[] expectedNicknames = { PrivateRoomSampleFactory.GuestNickname };

        await host.ConfirmKick();

        Assert.Equal(expectedNicknames, _roomClient.KickedNicknames);
    }

    [Fact]
    public async Task CancelKick_AskedForGuest_SendsNothing()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false));
        host.AskToKick(PrivateRoomSampleFactory.GuestNickname);

        host.CancelKick();

        Assert.Empty(_roomClient.KickedNicknames);
    }

    [Fact]
    public async Task JoinWithCode_TypedCode_SendsTheNormalizedCode()
    {
        PrivateRoomViewModel guest = CreateViewModel(PrivateRoomSampleFactory.GuestNickname);
        _roomClient.NextRoom = PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false);
        guest.OpenJoinDialog();
        guest.JoinDialog.CodeText = TypedCode;

        await guest.JoinWithCode();

        Assert.Equal(PrivateRoomSampleFactory.Code, _roomClient.JoinedCode);
    }

    [Fact]
    public async Task JoinWithCode_KickedBefore_ExplainsTheKick()
    {
        PrivateRoomViewModel guest = CreateViewModel(PrivateRoomSampleFactory.GuestNickname);
        _roomClient.Failure = CreateRejection(RoomRejection.Kicked);
        guest.OpenJoinDialog();
        guest.JoinDialog.CodeText = PrivateRoomSampleFactory.Code;

        await guest.JoinWithCode();

        Assert.Equal(Strings.RoomJoinKicked, guest.JoinDialog.ErrorText);
    }

    [Fact]
    public async Task JoinWithCode_OpenRoom_AsksTheGuestToGetReady()
    {
        PrivateRoomViewModel guest = await JoinRoomAsWaitingGuest();

        bool isReadyShown = guest.IsReadyShown;

        Assert.True(isReadyShown);
    }

    [Fact]
    public async Task RoomUpdated_HostChangedWalls_MarksTheWallsAsChanged()
    {
        PrivateRoomViewModel guest = await JoinRoomAsReadyGuest();
        var fewerWalls = new PrivateRoomOptions(
            PrivateRoomGameMode.Classic,
            PrivateRoomSampleFactory.FewWalls,
            PrivateRoomSampleFactory.FiveMinutes);

        _roomClient.RaiseRoomUpdated(PrivateRoomSampleFactory.CreateWithGuestAndOptions(fewerWalls));

        Assert.True(guest.IsWallsChanged);
    }

    [Fact]
    public async Task RoomUpdated_HostChangedWalls_LeavesTheClockUnmarked()
    {
        PrivateRoomViewModel guest = await JoinRoomAsReadyGuest();
        var fewerWalls = new PrivateRoomOptions(
            PrivateRoomGameMode.Classic,
            PrivateRoomSampleFactory.FewWalls,
            PrivateRoomSampleFactory.FiveMinutes);

        _roomClient.RaiseRoomUpdated(PrivateRoomSampleFactory.CreateWithGuestAndOptions(fewerWalls));

        Assert.False(guest.IsClockChanged);
    }

    [Fact]
    public async Task RoomUpdated_HostChangedWalls_ShowsTheSettingsNotice()
    {
        PrivateRoomViewModel guest = await JoinRoomAsReadyGuest();
        var fewerWalls = new PrivateRoomOptions(
            PrivateRoomGameMode.Classic,
            PrivateRoomSampleFactory.FewWalls,
            PrivateRoomSampleFactory.FiveMinutes);

        _roomClient.RaiseRoomUpdated(PrivateRoomSampleFactory.CreateWithGuestAndOptions(fewerWalls));

        Assert.True(guest.HasSettingsNotice);
    }

    [Fact]
    public async Task RoomClosed_Kicked_ReturnsToTheMenu()
    {
        PrivateRoomViewModel guest = await JoinRoomAsWaitingGuest();

        _roomClient.RaiseRoomClosed(PrivateRoomSampleFactory.Code, RoomCloseReason.Kicked);

        Assert.False(guest.IsInRoom);
    }

    [Fact]
    public async Task RoomClosed_Kicked_NamesTheHost()
    {
        PrivateRoomViewModel guest = await JoinRoomAsWaitingGuest();

        _roomClient.RaiseRoomClosed(PrivateRoomSampleFactory.Code, RoomCloseReason.Kicked);

        Assert.Contains(PrivateRoomSampleFactory.HostNickname, guest.NoticeText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Disconnected_InRoom_ExplainsWhyThePlayerLeft()
    {
        PrivateRoomViewModel guest = await JoinRoomAsWaitingGuest();

        _roomClient.RaiseDisconnected();

        Assert.Equal(Strings.RoomLostConnectionNotice, guest.NoticeText);
    }

    [Fact]
    public async Task Leave_Guest_TellsTheServer()
    {
        PrivateRoomViewModel guest = await JoinRoomAsWaitingGuest();

        await guest.Leave();

        Assert.Equal(1, _roomClient.LeaveCount);
    }

    [Fact]
    public async Task CopyCode_InRoom_SaysTheCodeWasCopied()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateHostAlone());

        host.CopyCode();

        Assert.True(host.IsCodeCopied);
    }

    [Fact]
    public async Task CopyCode_TwoSecondsLater_GoesBackToCopy()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateHostAlone());
        host.CopyCode();

        _timeProvider.Advance(_copiedTime);

        Assert.False(host.IsCodeCopied);
    }

    [Fact]
    public async Task StartMatch_EverySeatReady_AsksTheServer()
    {
        PrivateRoomViewModel host = await OpenRoomAsHost(PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: true));

        await host.StartMatch();

        Assert.Equal(1, _roomClient.StartCount);
    }

    [Fact]
    public async Task RestoreOwnRoom_PlayerInRoom_ShowsTheRoom()
    {
        PrivateRoomViewModel guest = CreateViewModel(PrivateRoomSampleFactory.GuestNickname);
        PrivateRoom room = PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false);
        _roomClient.OwnRoom = OwnRoom.CreateInRoom(room);

        await guest.RestoreOwnRoom();

        Assert.True(guest.IsInRoom);
    }

    private static RoomRejectedException CreateRejection(RoomRejection rejection)
    {
        return new RoomRejectedException(rejection, TimeSpan.Zero, new InvalidOperationException());
    }

    private PrivateRoomViewModel CreateViewModel(string nickname)
    {
        var session = new LoggedInSession(SessionToken, nickname, PreferredLanguage);
        var services = new PrivateRoomServices(_roomClient, _clipboard, _timeProvider);
        Func<PrivateRoomViewModel> createViewModel = () => new PrivateRoomViewModel(session, services);
        _viewModel = InlineUiContext.CreateViewModel(createViewModel);
        return _viewModel;
    }

    private async Task<PrivateRoomViewModel> OpenRoomAsHost(PrivateRoom room)
    {
        PrivateRoomViewModel host = CreateViewModel(PrivateRoomSampleFactory.HostNickname);
        _roomClient.NextRoom = room;
        host.OpenCreateSheet();
        await host.ConfirmSheet();
        return host;
    }

    private Task<PrivateRoomViewModel> JoinRoomAsWaitingGuest()
    {
        PrivateRoom room = PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: false);
        return JoinRoomAsGuest(room);
    }

    private Task<PrivateRoomViewModel> JoinRoomAsReadyGuest()
    {
        PrivateRoom room = PrivateRoomSampleFactory.CreateWithGuest(isGuestReady: true);
        return JoinRoomAsGuest(room);
    }

    private async Task<PrivateRoomViewModel> JoinRoomAsGuest(PrivateRoom room)
    {
        PrivateRoomViewModel guest = CreateViewModel(PrivateRoomSampleFactory.GuestNickname);
        _roomClient.NextRoom = room;
        guest.OpenJoinDialog();
        guest.JoinDialog.CodeText = PrivateRoomSampleFactory.Code;
        await guest.JoinWithCode();
        return guest;
    }
}
