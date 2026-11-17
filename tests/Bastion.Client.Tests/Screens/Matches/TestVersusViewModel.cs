using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestVersusViewModel
{
    private const int CountdownTickSeconds = 1;
    private const int OneCountdownFinished = 1;
    private const int NoCountdownFinished = 0;
    private const int NoActiveTimers = 0;

    private static readonly DateTimeOffset _shownAt = new DateTimeOffset(2026, 11, 5, 18, 1, 0, TimeSpan.Zero);
    private static readonly TimeSpan _countdownTick = TimeSpan.FromSeconds(CountdownTickSeconds);

    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_shownAt);
    private int _countdownFinishedCount;

    [Fact]
    public void Constructor_RivalListedFirst_ShowsOwnNickname()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        Assert.Equal(JoinedMatchSampleFactory.OwnNickname, viewModel.Own.Nickname);
    }

    [Fact]
    public void Constructor_RivalListedFirst_ShowsRivalNickname()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        VersusPlayer rival = viewModel.Rivals.Single();
        Assert.Equal(JoinedMatchSampleFactory.RivalNickname, rival.Nickname);
    }

    [Fact]
    public void Constructor_RankedRival_ShowsRivalElo()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchEloFormat,
            JoinedMatchSampleFactory.RivalEloRating);
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        VersusPlayer rival = viewModel.Rivals.Single();
        Assert.Equal(expectedText, rival.EloText);
    }

    [Fact]
    public void Constructor_ClassicFormat_ShowsModeText()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.VersusModeFormat,
            JoinedMatchSampleFactory.BoardSize,
            JoinedMatchSampleFactory.WallsPerPlayer);
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        Assert.Equal(expectedText, viewModel.ModeText);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_GivesOwnRedPawn()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.SecondTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        Assert.Equal(PawnSymbol.Red, viewModel.Own.PawnSymbol);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_GivesRivalBluePawn()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.SecondTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        VersusPlayer rival = viewModel.Rivals.Single();
        Assert.Equal(PawnSymbol.Blue, rival.PawnSymbol);
    }

    [Fact]
    public void Constructor_FourPlayers_ShowsRivalsInTurnOrder()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        string[] expectedNicknames =
        {
            JoinedMatchSampleFactory.RivalNickname,
            JoinedMatchSampleFactory.SecondRivalNickname,
            JoinedMatchSampleFactory.ThirdRivalNickname,
        };
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        IEnumerable<string> nicknames = viewModel.Rivals.Select(rival => rival.Nickname);
        Assert.Equal(expectedNicknames, nicknames);
    }

    [Fact]
    public void Constructor_Shown_StartsCountdownAtFullSeconds()
    {
        string expectedText = FormatCountdown(VersusViewModel.CountdownSeconds);
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);

        using VersusViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);

        Assert.Equal(expectedText, viewModel.CountdownText);
    }

    [Fact]
    public void CountdownTick_OneSecond_ShowsOneSecondLess()
    {
        using VersusViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        int secondsLeft = VersusViewModel.CountdownSeconds - CountdownTickSeconds;
        string expectedText = FormatCountdown(secondsLeft);

        AdvanceSeconds(CountdownTickSeconds);

        Assert.Equal(expectedText, viewModel.CountdownText);
    }

    [Fact]
    public void CountdownTick_AllSeconds_RaisesCountdownFinished()
    {
        using VersusViewModel viewModel = CreateViewModelListeningForFinish();

        AdvanceSeconds(VersusViewModel.CountdownSeconds);

        Assert.Equal(OneCountdownFinished, _countdownFinishedCount);
    }

    [Fact]
    public void CountdownTick_PastTheEnd_RaisesCountdownFinishedOnce()
    {
        using VersusViewModel viewModel = CreateViewModelListeningForFinish();
        int secondsPastTheEnd = VersusViewModel.CountdownSeconds + CountdownTickSeconds;

        AdvanceSeconds(secondsPastTheEnd);

        Assert.Equal(OneCountdownFinished, _countdownFinishedCount);
    }

    [Fact]
    public void CountdownTick_AllSeconds_StopsTimer()
    {
        using VersusViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        AdvanceSeconds(VersusViewModel.CountdownSeconds);

        int activeTimers = _timeProvider.CountActiveTimers();
        Assert.Equal(NoActiveTimers, activeTimers);
    }

    [Fact]
    public void Dispose_Counting_StopsTimer()
    {
        VersusViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        viewModel.Dispose();

        int activeTimers = _timeProvider.CountActiveTimers();
        Assert.Equal(NoActiveTimers, activeTimers);
    }

    [Fact]
    public void CountdownTick_AfterDispose_NeverRaisesCountdownFinished()
    {
        VersusViewModel viewModel = CreateViewModelListeningForFinish();
        viewModel.Dispose();

        AdvanceSeconds(VersusViewModel.CountdownSeconds);

        Assert.Equal(NoCountdownFinished, _countdownFinishedCount);
    }

    private static string FormatCountdown(int secondsLeft)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.VersusStartsInFormat, secondsLeft);
    }

    private void AdvanceSeconds(int seconds)
    {
        for (int second = 0; second < seconds; second++)
        {
            _timeProvider.Advance(_countdownTick);
        }
    }

    private VersusViewModel CreateViewModelListeningForFinish()
    {
        VersusViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        viewModel.CountdownFinished += HandleCountdownFinished;
        return viewModel;
    }

    private VersusViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        Func<VersusViewModel> createViewModel = () => new VersusViewModel(joinedMatch, _timeProvider);
        return InlineUiContext.CreateViewModel(createViewModel);
    }

    private void HandleCountdownFinished(object? sender, EventArgs e)
    {
        _countdownFinishedCount++;
    }
}
