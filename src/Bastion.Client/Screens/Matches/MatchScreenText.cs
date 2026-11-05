using System;
using System.Globalization;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.Matches;

public static class MatchScreenText
{
    private const string ClockFormat = "{0}:{1:00}";
    private const int WholeMinuteSeconds = 0;

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

    public static string FormatWallCount(byte wallCount)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.MatchWallsFormat, wallCount);
    }

    public static string FormatMode(byte boardSize, byte wallsPerPlayer)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.VersusModeFormat, boardSize, wallsPerPlayer);
    }
}
