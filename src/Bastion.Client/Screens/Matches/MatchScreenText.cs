using System;
using System.Globalization;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.Matches;

public static class MatchScreenText
{
    private const string ClockFormat = "{0}:{1:00}";
    private const int WholeMinuteSeconds = 0;
    private const int SecondsPerMinute = 60;

    public static string FormatElo(short? eloRating)
    {
        if (eloRating is not short elo)
        {
            return string.Empty;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchEloFormat, elo);
    }

    public static string FormatClock(byte clockMinutes)
    {
        return string.Format(CultureInfo.InvariantCulture, ClockFormat, clockMinutes, WholeMinuteSeconds);
    }

    public static string FormatClock(TimeSpan remainingClock)
    {
        double roundedUpSeconds = Math.Ceiling(remainingClock.TotalSeconds);
        int remainingSeconds = (int)roundedUpSeconds;
        int minutes = remainingSeconds / SecondsPerMinute;
        int seconds = remainingSeconds % SecondsPerMinute;
        return string.Format(CultureInfo.InvariantCulture, ClockFormat, minutes, seconds);
    }

    public static string FormatElapsed(TimeSpan elapsedTime)
    {
        double roundedDownSeconds = Math.Floor(Math.Max(elapsedTime.TotalSeconds, 0));
        int elapsedSeconds = (int)roundedDownSeconds;
        int minutes = elapsedSeconds / SecondsPerMinute;
        int seconds = elapsedSeconds % SecondsPerMinute;
        return string.Format(CultureInfo.InvariantCulture, ClockFormat, minutes, seconds);
    }

    public static string FormatWallCount(byte wallCount)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.MatchWallsFormat, wallCount);
    }

    public static string FormatMode(byte boardSize, byte wallsPerPlayer)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.VersusModeFormat, boardSize, wallsPerPlayer);
    }
}
