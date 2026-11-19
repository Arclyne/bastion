using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchConnectionViewModel : IDisposable
{
    private const int OtherMatchId = 40;
    private const string HalfMinuteLeftText = "0:30";

    private static readonly DateTimeOffset _startedAt = new DateTimeOffset(2026, 11, 19, 18, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _halfMinute = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan _fiftyFiveSeconds = TimeSpan.FromSeconds(55);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly FakeNotificationClient _notificationClient = new FakeNotificationClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_startedAt);
    private readonly List<IDisposable> _viewModels = [];
    private readonly JoinedMatch _joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
    private readonly MatchPlayViewModel _matchPlay;
    private readonly MatchConnectionViewModel _connection;

    public TestMatchConnectionViewModel()
    {
        Func<MatchPlayViewModel> createMatchPlay = () => new MatchPlayViewModel(
            _joinedMatch,
            _matchClient,
            _timeProvider);
        _matchPlay = InlineUiContext.CreateViewModel(createMatchPlay);
        var services = new MatchConnectionServices(_notificationClient, _matchClient, _timeProvider);
        Func<MatchConnectionViewModel> createConnection = () => new MatchConnectionViewModel(
            _joinedMatch,
            _matchPlay,
            services);
        _connection = InlineUiContext.CreateViewModel(createConnection);
        _viewModels.Add(_connection);
        _viewModels.Add(_matchPlay);
    }

    public void Dispose()
    {
        _viewModels.ForEach(viewModel => viewModel.Dispose());
    }

    [Fact]
    public void Disconnected_DuringMatch_ShowsReconnecting()
    {
        _notificationClient.RaiseDisconnected();

        Assert.True(_connection.IsReconnecting);
    }

    [Fact]
    public void Disconnected_DuringMatch_PutsTheBoardOffline()
    {
        _notificationClient.RaiseDisconnected();

        Assert.True(_matchPlay.IsOffline);
    }

    [Fact]
    public async Task ReconnectNow_ServerBack_HidesReconnecting()
    {
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.False(_connection.IsReconnecting);
    }

    [Fact]
    public async Task ReconnectNow_ServerBack_ConnectsTheNotificationsAgain()
    {
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.Equal(1, _notificationClient.ConnectCount);
    }

    [Fact]
    public async Task ReconnectNow_ServerBack_TellsHowManyMovesWerePlayed()
    {
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.True(_connection.HasInformationToast);
    }

    [Fact]
    public async Task ReconnectNow_ServerBack_PutsTheBoardOnline()
    {
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch);
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.False(_matchPlay.IsOffline);
    }

    [Fact]
    public async Task ReconnectNow_MatchClaimedWhileAway_ShowsTheEnd()
    {
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateEndedByRivalClaim(_joinedMatch);
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.True(_matchPlay.IsMatchOver);
    }

    [Fact]
    public async Task ReconnectNow_MatchClaimedWhileAway_RemembersThePlayerWasAway()
    {
        _matchClient.Rejoined = RejoinedMatchSampleFactory.CreateEndedByRivalClaim(_joinedMatch);
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.True(_connection.WasAwayAtEnd(Match.FirstTurnOrder));
    }

    [Fact]
    public void OpponentDisconnected_JoinedMatch_ShowsTheAwayRival()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        Assert.True(_connection.HasAwayRival);
    }

    [Fact]
    public void OpponentDisconnected_OtherMatch_ShowsNoAwayRival()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(OtherMatchId));

        Assert.False(_connection.HasAwayRival);
    }

    [Fact]
    public async Task ReconnectNow_ServerDown_KeepsReconnecting()
    {
        _notificationClient.ConnectFailure = new ServerUnreachableException();
        _notificationClient.RaiseDisconnected();

        await _connection.ReconnectNow();

        Assert.True(_connection.IsReconnecting);
    }

    [Fact]
    public void OpponentDisconnected_HalfAMinuteAway_CountsDownTheRest()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        _timeProvider.Advance(_halfMinute);

        Assert.Equal(HalfMinuteLeftText, _connection.CountdownText);
    }

    [Fact]
    public void OpponentDisconnected_HalfAMinuteAway_IsNotEnding()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        _timeProvider.Advance(_halfMinute);

        Assert.False(_connection.IsCountdownEnding);
    }

    [Fact]
    public void OpponentDisconnected_LastSeconds_MarksTheCountdownEnding()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        _timeProvider.Advance(_fiftyFiveSeconds);

        Assert.True(_connection.IsCountdownEnding);
    }

    [Fact]
    public void OpponentReconnected_AwayRival_HidesTheAwayRival()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        _matchClient.RaiseOpponentReconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        Assert.False(_connection.HasAwayRival);
    }

    [Fact]
    public void OpponentReconnected_AwayRival_ShowsTheReturn()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        _matchClient.RaiseOpponentReconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        Assert.True(_connection.HasSuccessToast);
    }

    [Fact]
    public void WasAwayAtEnd_RivalAway_ReturnsTrue()
    {
        _matchClient.RaiseOpponentDisconnected(CreateConnection(JoinedMatchSampleFactory.MatchId));

        bool wasAway = _connection.WasAwayAtEnd(Match.SecondTurnOrder);

        Assert.True(wasAway);
    }

    [Fact]
    public void TrackRejoinedPlayers_RivalAway_ShowsTheAwayRival()
    {
        RejoinedMatch rejoinedMatch = RejoinedMatchSampleFactory.CreateWithRivalConnection(
            _joinedMatch,
            isRivalConnected: false);

        _connection.TrackRejoinedPlayers(rejoinedMatch);

        Assert.True(_connection.HasAwayRival);
    }

    [Fact]
    public void MatchTakenOver_DuringMatch_ShowsTakenOver()
    {
        _matchClient.RaiseMatchTakenOver();

        Assert.True(_connection.IsTakenOver);
    }

    [Fact]
    public void GoToMenu_TakenOver_AsksForTheMenu()
    {
        bool isMenuRequested = false;
        _connection.ReturnToMenuRequested += (_, _) => isMenuRequested = true;
        _matchClient.RaiseMatchTakenOver();

        _connection.GoToMenu();

        Assert.True(isMenuRequested);
    }

    private static OpponentConnectionEventArgs CreateConnection(int matchId)
    {
        return new OpponentConnectionEventArgs
        {
            MatchId = matchId,
            TurnOrder = Match.SecondTurnOrder,
            OccurredAt = _startedAt.UtcDateTime,
        };
    }
}
