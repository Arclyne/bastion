using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public class TestVersusViewModel : IDisposable
{
    private const int CountdownTickSeconds = 1;

    private static readonly DateTimeOffset _shownAt = new DateTimeOffset(2026, 11, 5, 18, 1, 0, TimeSpan.Zero);

    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_shownAt);
    private readonly VersusViewModel _viewModel;
    private int _countdownFinishedCount;

    public TestVersusViewModel()
    {
        _viewModel = CreateViewModel(Match.FirstTurnOrder);
        _viewModel.CountdownFinished += HandleCountdownFinished;
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public void Constructor_RivalListedFirst_ShowsOwnNickname()
    {
        Assert.Equal(JoinedMatchSamples.OwnNickname, _viewModel.OwnNickname);
    }

    [Fact]
    public void Constructor_RivalListedFirst_ShowsRivalNickname()
    {
        Assert.Equal(JoinedMatchSamples.RivalNickname, _viewModel.RivalNickname);
    }

    [Fact]
    public void Constructor_RankedRival_ShowsRivalElo()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchEloFormat,
            JoinedMatchSamples.RivalEloRating);

        Assert.Equal(expectedText, _viewModel.RivalEloText);
    }

    [Fact]
    public void Constructor_ClassicFormat_ShowsModeText()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.VersusModeFormat,
            JoinedMatchSamples.BoardSize,
            JoinedMatchSamples.WallsPerPlayer);

        Assert.Equal(expectedText, _viewModel.ModeText);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_GivesOwnRedPawn()
    {
        using VersusViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        Assert.Equal(PawnSymbol.Red, viewModel.OwnPawnSymbol);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_GivesRivalBluePawn()
    {
        using VersusViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        Assert.Equal(PawnSymbol.Blue, viewModel.RivalPawnSymbol);
    }

    [Fact]
    public void Constructor_Shown_StartsCountdownAtFullSeconds()
    {
        string expectedText = FormatCountdown(VersusViewModel.CountdownSeconds);

        Assert.Equal(expectedText, _viewModel.CountdownText);
    }

    [Fact]
    public void CountdownTick_OneSecond_ShowsOneSecondLess()
    {
        string expectedText = FormatCountdown(VersusViewModel.CountdownSeconds - CountdownTickSeconds);

        AdvanceSeconds(CountdownTickSeconds);

        Assert.Equal(expectedText, _viewModel.CountdownText);
    }

    [Fact]
    public void CountdownTick_AllSeconds_RaisesCountdownFinished()
    {
        AdvanceSeconds(VersusViewModel.CountdownSeconds);

        Assert.Equal(1, _countdownFinishedCount);
    }

    [Fact]
    public void CountdownTick_PastTheEnd_RaisesCountdownFinishedOnce()
    {
        AdvanceSeconds(VersusViewModel.CountdownSeconds + CountdownTickSeconds);

        Assert.Equal(1, _countdownFinishedCount);
    }

    [Fact]
    public void CountdownTick_AllSeconds_StopsTimer()
    {
        AdvanceSeconds(VersusViewModel.CountdownSeconds);

        Assert.Equal(0, _timeProvider.CountActiveTimers());
    }

    [Fact]
    public void Dispose_Counting_StopsTimer()
    {
        _viewModel.Dispose();

        Assert.Equal(0, _timeProvider.CountActiveTimers());
    }

    [Fact]
    public void Dispose_Counting_NeverRaisesCountdownFinished()
    {
        _viewModel.Dispose();

        AdvanceSeconds(VersusViewModel.CountdownSeconds);

        Assert.Equal(0, _countdownFinishedCount);
    }

    private static string FormatCountdown(int secondsLeft)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.VersusStartsInFormat, secondsLeft);
    }

    private VersusViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSamples.CreateWithOwnTurnOrder(ownTurnOrder);
        return InlineUiContext.CreateViewModel(() => new VersusViewModel(joinedMatch, _timeProvider));
    }

    private void AdvanceSeconds(int seconds)
    {
        for (int second = 0; second < seconds; second++)
        {
            _timeProvider.Advance(TimeSpan.FromSeconds(CountdownTickSeconds));
        }
    }

    private void HandleCountdownFinished(object? sender, EventArgs e)
    {
        _countdownFinishedCount++;
    }
}
