using System;
using Bastion.Client.Screens.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchScreenText
{
    private const byte TenMinutes = 10;
    private const string TenMinuteClockText = "10:00";
    private const string FiveMinuteClockText = "5:00";
    private const string ZeroClockText = "0:00";
    private const string OneMinuteFiveClockText = "1:05";

    private static readonly TimeSpan _almostFiveMinutes = new TimeSpan(0, 0, 4, 59, 200);
    private static readonly TimeSpan _sixtyFiveSeconds = TimeSpan.FromSeconds(65);

    [Fact]
    public void FormatElo_NoRating_ReturnsEmptyText()
    {
        string text = MatchScreenText.FormatElo(eloRating: null);

        Assert.Empty(text);
    }

    [Fact]
    public void FormatClock_TenMinutes_ShowsMinutesAndZeroSeconds()
    {
        string text = MatchScreenText.FormatClock(TenMinutes);

        Assert.Equal(TenMinuteClockText, text);
    }

    [Fact]
    public void FormatClock_PartOfASecondLeft_RoundsUpTheSecond()
    {
        string text = MatchScreenText.FormatClock(_almostFiveMinutes);

        Assert.Equal(FiveMinuteClockText, text);
    }

    [Fact]
    public void FormatClock_NothingLeft_ShowsZero()
    {
        string text = MatchScreenText.FormatClock(TimeSpan.Zero);

        Assert.Equal(ZeroClockText, text);
    }

    [Fact]
    public void FormatClock_SixtyFiveSeconds_ShowsOneMinuteAndFiveSeconds()
    {
        string text = MatchScreenText.FormatClock(_sixtyFiveSeconds);

        Assert.Equal(OneMinuteFiveClockText, text);
    }
}
