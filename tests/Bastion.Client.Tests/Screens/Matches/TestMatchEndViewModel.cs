using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchEndViewModel : IDisposable
{
    private const string OwnWinEloText = "1210 » 1222";
    private const string OwnWinEloChangeText = "+12";
    private const string OwnLossEloChangeText = "-20";
    private const string PlayedTimeText = "12:48";
    private const int OneSearchRequest = 1;

    private static readonly DateTimeOffset _startedAt = new DateTimeOffset(2026, 11, 19, 18, 0, 0, TimeSpan.Zero);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_startedAt);
    private readonly List<IDisposable> _createdViewModels = [];
    private int _searchAgainRequestCount;

    public void Dispose()
    {
        foreach (IDisposable viewModel in _createdViewModels)
        {
            viewModel.Dispose();
        }
    }

    [Fact]
    public void MatchEnded_OwnGoalWin_ShowsDialog()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.True(viewModel.IsShown);
    }

    [Fact]
    public void MatchEnded_OwnGoalWin_ShowsVictoryTitle()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(Strings.MatchEndVictoryTitle, viewModel.TitleText);
    }

    [Fact]
    public void MatchEnded_RivalGoalWin_ShowsDefeatTitle()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.SecondTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateRivalGoalWin(Match.SecondTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(Strings.MatchEndDefeatTitle, viewModel.TitleText);
    }

    [Fact]
    public void MatchEnded_OwnGoalWin_ShowsOwnGoalReason()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(Strings.MatchEndOwnGoal, viewModel.ReasonText);
    }

    [Fact]
    public void MatchEnded_RivalAbandoned_NamesRivalInReason()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchEndRivalAbandonmentFormat,
            JoinedMatchSampleFactory.RivalNickname);
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateRivalAbandonment(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(expectedText, viewModel.ReasonText);
    }

    [Fact]
    public void MatchEnded_OwnAbandonment_ShowsOwnAbandonmentReason()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnAbandonment(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(Strings.MatchEndOwnAbandonment, viewModel.ReasonText);
    }

    [Fact]
    public void MatchEnded_OwnAbandonment_ShowsEloLoss()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnAbandonment(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.True(viewModel.HasEloLoss);
    }

    [Fact]
    public void MatchEnded_RivalAbandoned_MarksRivalRowLeft()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateRivalAbandonment(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        MatchEndRow rivalRow = viewModel.Rows.First(row => !row.IsOwn);
        Assert.Equal(Strings.MatchEndResultLeft, rivalRow.ResultText);
    }

    [Fact]
    public void MatchEnded_Ranked_ShowsRankedKind()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(Strings.MatchEndRankedKind, viewModel.KindText);
    }

    [Fact]
    public void MatchEnded_OwnWin_ShowsEloTransition()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(OwnWinEloText, viewModel.EloText);
    }

    [Fact]
    public void MatchEnded_OwnWin_ShowsSignedEloGain()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(OwnWinEloChangeText, viewModel.EloChangeText);
    }

    [Fact]
    public void MatchEnded_OwnLoss_ShowsSignedEloLoss()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.SecondTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateRivalGoalWin(Match.SecondTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(OwnLossEloChangeText, viewModel.EloChangeText);
    }

    [Fact]
    public void MatchEnded_OwnLoss_ShowsEloLoss()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.SecondTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateRivalGoalWin(Match.SecondTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.True(viewModel.HasEloLoss);
    }

    [Fact]
    public void MatchEnded_PrivateMatch_HidesElo()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreatePrivateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.False(viewModel.HasElo);
    }

    [Fact]
    public void MatchEnded_PrivateMatch_ShowsPrivateNotice()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreatePrivateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.True(viewModel.HasPrivateNotice);
    }

    [Fact]
    public void MatchEnded_AnyEnd_ShowsSummary()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchEndSummaryFormat,
            MatchEndSampleFactory.MoveCount,
            PlayedTimeText,
            MatchEndSampleFactory.OwnRemainingWalls);
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateOwnGoalWin(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.Equal(expectedText, viewModel.SummaryText);
    }

    [Fact]
    public void OwnParticipationEnded_SecondPlace_ShowsPlacementTitle()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchEndPlacementFormat,
            MatchEndSampleFactory.SecondPlacement);
        MatchEndViewModel viewModel = CreateFourPlayerViewModel();
        ParticipationEndedEventArgs participationEnd = MatchEndSampleFactory.CreateOwnThirdTurnSecondPlace();

        _matchClient.RaiseParticipationEnded(participationEnd);

        Assert.Equal(expectedText, viewModel.TitleText);
    }

    [Fact]
    public void OwnParticipationEnded_SecondPlace_SaysMatchGoesOn()
    {
        string goesOn = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchEndGoesOnFormat,
            MatchEndSampleFactory.TwoActivePlayers);
        string expectedText = string.Join(' ', Strings.MatchEndOwnGoal, goesOn);
        MatchEndViewModel viewModel = CreateFourPlayerViewModel();
        ParticipationEndedEventArgs participationEnd = MatchEndSampleFactory.CreateOwnThirdTurnSecondPlace();

        _matchClient.RaiseParticipationEnded(participationEnd);

        Assert.Equal(expectedText, viewModel.ReasonText);
    }

    [Fact]
    public void OwnParticipationEnded_SecondPlace_ShowsWhoIsStillPlaying()
    {
        string[] expectedResults =
        [
            Strings.MatchEndResultGoal,
            Strings.MatchEndResultPlaying,
            Strings.MatchEndResultGoal,
            Strings.MatchEndResultPlaying,
        ];
        MatchEndViewModel viewModel = CreateFourPlayerViewModel();
        ParticipationEndedEventArgs participationEnd = MatchEndSampleFactory.CreateOwnThirdTurnSecondPlace();

        _matchClient.RaiseParticipationEnded(participationEnd);

        IEnumerable<string> results = viewModel.Rows.Select(row => row.ResultText);
        Assert.Equal(expectedResults, results);
    }

    [Fact]
    public void OtherParticipationEnded_RivalLeft_KeepsDialogClosed()
    {
        MatchEndViewModel viewModel = CreateFourPlayerViewModel();
        ParticipationEndedEventArgs departure = MatchEndSampleFactory.CreateDeparture(
            JoinedMatchSampleFactory.MatchId,
            Match.FourthTurnOrder,
            EndedMatchReason.Abandonment);

        _matchClient.RaiseParticipationEnded(departure);

        Assert.False(viewModel.IsShown);
    }

    [Fact]
    public void RequestSearchAgain_MatchEnded_RequestsSearch()
    {
        MatchEndViewModel viewModel = CreateTwoPlayerViewModel(Match.FirstTurnOrder);
        viewModel.SearchAgainRequested += HandleSearchAgainRequested;

        viewModel.RequestSearchAgain();

        Assert.Equal(OneSearchRequest, _searchAgainRequestCount);
    }

    private MatchEndViewModel CreateTwoPlayerViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        return CreateViewModel(joinedMatch);
    }

    private MatchEndViewModel CreateFourPlayerViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        return CreateViewModel(joinedMatch);
    }

    private MatchEndViewModel CreateViewModel(JoinedMatch joinedMatch)
    {
        Func<MatchPlayViewModel> createPlayViewModel = () => new MatchPlayViewModel(
            joinedMatch,
            _matchClient,
            _timeProvider);
        MatchPlayViewModel matchPlayViewModel = InlineUiContext.CreateViewModel(createPlayViewModel);
        var viewModel = new MatchEndViewModel(joinedMatch, matchPlayViewModel);
        _createdViewModels.Add(viewModel);
        _createdViewModels.Add(matchPlayViewModel);
        return viewModel;
    }

    private void HandleSearchAgainRequested(object? sender, EventArgs e)
    {
        _searchAgainRequestCount++;
    }
}
