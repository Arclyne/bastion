using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchAbandonmentViewModel : IDisposable
{
    private const int OtherMatchId = 40;
    private const int OneReturnRequest = 1;
    private const int NoReturnRequests = 0;
    private const short OwnInitialElo = 1210;
    private const short OwnFinalElo = 1190;
    private const int EloLoss = 20;
    private const byte LastOfFourPlacement = 4;
    private const int ThreePlayersLeft = 3;

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly MatchAbandonmentViewModel _viewModel;
    private int _returnToMenuRequestCount;

    public TestMatchAbandonmentViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        Func<MatchAbandonmentViewModel> createViewModel = () => new MatchAbandonmentViewModel(
            joinedMatch,
            _matchClient);
        _viewModel = InlineUiContext.CreateViewModel(createViewModel);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public async Task AbandonMatch_Accepted_AbandonsJoinedMatch()
    {
        await _viewModel.AbandonMatch();

        Assert.Equal(JoinedMatchSampleFactory.MatchId, _matchClient.LastAbandonedMatchId);
    }

    [Fact]
    public async Task AbandonMatch_Accepted_StaysToShowResult()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;

        await _viewModel.AbandonMatch();

        Assert.Equal(NoReturnRequests, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_MatchAlreadyOver_RequestsReturnToMenu()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        _matchClient.AbandonFailure = new MatchNotJoinedException();

        await _viewModel.AbandonMatch();

        Assert.Equal(OneReturnRequest, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_ServerUnreachable_ShowsAbandonFailed()
    {
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.Equal(Strings.MatchAbandonFailed, _viewModel.StatusText);
    }

    [Fact]
    public async Task AbandonMatch_ServerUnreachable_StaysInMatch()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.Equal(NoReturnRequests, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_ServerUnreachable_AllowsAnotherTry()
    {
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.True(_viewModel.CanAbandon);
    }

    [Fact]
    public async Task OpenConfirmation_AnyMatch_OpensDialog()
    {
        await _viewModel.OpenConfirmation();

        Assert.True(_viewModel.IsConfirming);
    }

    [Fact]
    public async Task OpenConfirmation_AnyMatch_DoesNotAbandon()
    {
        await _viewModel.OpenConfirmation();

        Assert.Null(_matchClient.LastAbandonedMatchId);
    }

    [Fact]
    public async Task OpenConfirmation_TwoPlayers_NamesRivalInConsequence()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchAbandonConfirmDuelFormat,
            JoinedMatchSampleFactory.RivalNickname);

        await _viewModel.OpenConfirmation();

        Assert.Equal(expectedText, _viewModel.ConsequenceText);
    }

    [Fact]
    public async Task OpenConfirmation_RankedPreview_ShowsEloLoss()
    {
        _matchClient.Preview = CreateRankedPreview();
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchAbandonEloLossFormat,
            EloLoss,
            OwnInitialElo,
            OwnFinalElo);

        await _viewModel.OpenConfirmation();

        Assert.Equal(expectedText, _viewModel.PreviewText);
    }

    [Fact]
    public async Task OpenConfirmation_RankedPreview_ShowsLossTone()
    {
        _matchClient.Preview = CreateRankedPreview();

        await _viewModel.OpenConfirmation();

        Assert.True(_viewModel.IsPreviewLoss);
    }

    [Fact]
    public async Task OpenConfirmation_PrivatePreview_ShowsPrivateNotice()
    {
        await _viewModel.OpenConfirmation();

        Assert.Equal(Strings.MatchEndPrivateNotice, _viewModel.PreviewText);
    }

    [Fact]
    public async Task OpenConfirmation_PreviewFails_ShowsUnknownLoss()
    {
        _matchClient.PreviewFailure = new ServerUnreachableException();

        await _viewModel.OpenConfirmation();

        Assert.Equal(Strings.MatchAbandonEloUnknown, _viewModel.PreviewText);
    }

    [Fact]
    public async Task OpenConfirmation_FourPlayerPreview_DescribesPlacement()
    {
        _matchClient.Preview = new PreviewedAbandonment
        {
            IsRanked = false,
            InitialElo = null,
            FinalElo = null,
            Placement = LastOfFourPlacement,
            ActivePlayerCount = ThreePlayersLeft,
        };
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchAbandonConfirmGroupFormat,
            LastOfFourPlacement,
            ThreePlayersLeft);

        await _viewModel.OpenConfirmation();

        Assert.Equal(expectedText, _viewModel.ConsequenceText);
    }

    [Fact]
    public async Task CancelConfirmation_Open_ClosesDialog()
    {
        await _viewModel.OpenConfirmation();

        _viewModel.CancelConfirmation();

        Assert.False(_viewModel.IsConfirming);
    }

    [Fact]
    public async Task ConfirmAbandonment_Open_AbandonsJoinedMatch()
    {
        await _viewModel.OpenConfirmation();

        await _viewModel.ConfirmAbandonment();

        Assert.Equal(JoinedMatchSampleFactory.MatchId, _matchClient.LastAbandonedMatchId);
    }

    [Fact]
    public async Task ConfirmAbandonment_Open_ClosesDialog()
    {
        await _viewModel.OpenConfirmation();

        await _viewModel.ConfirmAbandonment();

        Assert.False(_viewModel.IsConfirming);
    }

    [Fact]
    public void ParticipationEnded_OtherMatch_IgnoresNotice()
    {
        ParticipationEndedEventArgs departure = MatchEndSampleFactory.CreateDeparture(
            OtherMatchId,
            Match.SecondTurnOrder,
            EndedMatchReason.Abandonment);

        _matchClient.RaiseParticipationEnded(departure);

        Assert.False(_viewModel.HasPlayerLeft);
    }

    [Fact]
    public void ParticipationEnded_RivalLeft_ShowsPlayerLeftNotice()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerLeftFormat,
            JoinedMatchSampleFactory.RivalNickname);

        RaiseRivalDeparture(EndedMatchReason.Abandonment);

        Assert.Equal(expectedText, _viewModel.PlayerLeftText);
    }

    [Fact]
    public void ParticipationEnded_RivalReachedGoal_ShowsReachedGoalNotice()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerReachedGoalFormat,
            JoinedMatchSampleFactory.RivalNickname);

        RaiseRivalDeparture(EndedMatchReason.Goal);

        Assert.Equal(expectedText, _viewModel.PlayerLeftText);
    }

    [Fact]
    public void ParticipationEnded_RivalLeft_RemembersDeparture()
    {
        string[] expectedNicknames = { JoinedMatchSampleFactory.RivalNickname };

        RaiseRivalDeparture(EndedMatchReason.Abandonment);

        IEnumerable<string> nicknames = _viewModel.Departures.Select(departure => departure.Nickname);
        Assert.Equal(expectedNicknames, nicknames);
    }

    [Fact]
    public void ParticipationEnded_OwnDeparture_ShowsNoNotice()
    {
        ParticipationEndedEventArgs departure = MatchEndSampleFactory.CreateDeparture(
            JoinedMatchSampleFactory.MatchId,
            Match.FirstTurnOrder,
            EndedMatchReason.Goal);

        _matchClient.RaiseParticipationEnded(departure);

        Assert.False(_viewModel.HasPlayerLeft);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesParticipationEndedHandler()
    {
        _viewModel.Dispose();

        bool hasSubscribers = _matchClient.HasParticipationEndedSubscribers();
        Assert.False(hasSubscribers);
    }

    private static PreviewedAbandonment CreateRankedPreview()
    {
        return new PreviewedAbandonment
        {
            IsRanked = true,
            InitialElo = OwnInitialElo,
            FinalElo = OwnFinalElo,
            Placement = null,
            ActivePlayerCount = 0,
        };
    }

    private void RaiseRivalDeparture(EndedMatchReason endReason)
    {
        ParticipationEndedEventArgs departure = MatchEndSampleFactory.CreateDeparture(
            JoinedMatchSampleFactory.MatchId,
            Match.SecondTurnOrder,
            endReason);
        _matchClient.RaiseParticipationEnded(departure);
    }

    private void HandleReturnToMenuRequested(object? sender, EventArgs e)
    {
        _returnToMenuRequestCount++;
    }
}
