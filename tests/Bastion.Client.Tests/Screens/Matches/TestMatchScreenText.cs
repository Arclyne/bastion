using Bastion.Client.Screens.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchScreenText
{
    private const byte TenMinutes = 10;
    private const string TenMinuteClockText = "10:00";

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
}
