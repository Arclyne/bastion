using System;
using System.Threading.Tasks;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Screens.MainMenu;
using Bastion.Client.Screens.MainMenu.Rooms;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Client.Tests.Screens.Matches;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu;

public sealed class TestMainMenuViewModel : IDisposable
{
    private const string SessionToken = "session-token";
    private const string PreferredLanguage = "es-MX";

    private const int WaitStepMilliseconds = 10;
    private const int MaximumWaitSteps = 200;

    private static readonly TimeSpan _firstRetryDelay = TimeSpan.FromSeconds(2);
    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 19, 18, 0, 0, TimeSpan.Zero);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly FakeNotificationClient _notificationClient = new FakeNotificationClient();
    private readonly FakeRoomClient _roomClient = new FakeRoomClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_now);
    private readonly JoinedMatch _joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
    private readonly MainMenuViewModel _viewModel;

    public TestMainMenuViewModel()
    {
        var session = new LoggedInSession(SessionToken, JoinedMatchSampleFactory.OwnNickname, PreferredLanguage);
        var services = new MainMenuServices(_notificationClient, _matchClient, _timeProvider);
        var roomServices = new PrivateRoomServices(_roomClient, new FakeClipboard(), _timeProvider);
        var privateRoom = new PrivateRoomViewModel(session, roomServices);
        Func<MainMenuViewModel> createViewModel = () => new MainMenuViewModel(session, services)
        {
            PrivateRoom = privateRoom,
        };
        _viewModel = InlineUiContext.CreateViewModel(createViewModel);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
        _viewModel.PrivateRoom.Dispose();
    }

    [Fact]
    public void Disconnected_InMenu_ShowsOffline()
    {
        _notificationClient.RaiseDisconnected();

        Assert.True(_viewModel.IsOffline);
    }

    [Fact]
    public async Task ConnectNotifications_ServerDown_ShowsOffline()
    {
        _notificationClient.ConnectFailure = new ServerUnreachableException();

        await _viewModel.ConnectNotifications();

        Assert.True(_viewModel.IsOffline);
    }

    [Fact]
    public async Task ConnectNotifications_ServerBack_HidesOffline()
    {
        _notificationClient.ConnectFailure = new ServerUnreachableException();
        await _viewModel.ConnectNotifications();
        _notificationClient.ConnectFailure = null;

        await _viewModel.ConnectNotifications();

        Assert.False(_viewModel.IsOffline);
    }

    [Fact]
    public async Task Disconnected_FirstDelayPassed_ConnectsAgain()
    {
        _notificationClient.RaiseDisconnected();

        _timeProvider.Advance(_firstRetryDelay);

        await WaitUntilConnected();
        Assert.Equal(1, _notificationClient.ConnectCount);
    }

    [Fact]
    public async Task ReturnToOngoingMatch_MatchInProgress_AnnouncesTheRejoinedMatch()
    {
        GiveOngoingMatch();
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);
        RejoinedMatch? resumedMatch = null;
        _viewModel.MatchResumed += (_, e) => resumedMatch = e.RejoinedMatch;

        await _viewModel.ReturnToOngoingMatch();

        Assert.Same(_matchClient.Rejoined, resumedMatch);
    }

    [Fact]
    public async Task ReturnToOngoingMatch_MatchInProgress_NamesTheRival()
    {
        GiveOngoingMatch();
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);

        await _viewModel.ReturnToOngoingMatch();

        Assert.Contains(JoinedMatchSampleFactory.RivalNickname, _viewModel.ReturningDetailText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnToOngoingMatch_MatchInProgress_HidesTheReturnWhenDone()
    {
        GiveOngoingMatch();
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);

        await _viewModel.ReturnToOngoingMatch();

        Assert.False(_viewModel.IsReturningToMatch);
    }

    [Fact]
    public async Task ReturnToOngoingMatch_NoMatch_RejoinsNothing()
    {
        await _viewModel.ReturnToOngoingMatch();

        Assert.Equal(0, _matchClient.RejoinCount);
    }

    [Fact]
    public async Task ReturnToOngoingMatch_LookupFails_RejoinsNothing()
    {
        _matchClient.OngoingMatchFailure = new ServerUnreachableException();

        await _viewModel.ReturnToOngoingMatch();

        Assert.Equal(0, _matchClient.RejoinCount);
    }

    [Fact]
    public async Task ReturnToOngoingMatch_MatchAlreadyOver_AnnouncesNothing()
    {
        GiveOngoingMatch();
        _matchClient.RejoinFailure = new MatchNotJoinedException();
        bool isResumed = false;
        _viewModel.MatchResumed += (_, _) => isResumed = true;

        await _viewModel.ReturnToOngoingMatch();

        Assert.False(isResumed);
    }

    private async Task WaitUntilConnected()
    {
        for (int step = 0; step < MaximumWaitSteps && _notificationClient.ConnectCount == 0; step++)
        {
            await Task.Delay(WaitStepMilliseconds);
        }
    }

    private void GiveOngoingMatch()
    {
        RejoinedMatch ongoingMatch = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);
        _matchClient.OngoingMatch = new FoundOngoingMatch
        {
            Match = ongoingMatch,
        };
    }
}
