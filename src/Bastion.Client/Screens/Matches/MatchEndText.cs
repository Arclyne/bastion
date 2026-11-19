using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Screens.Matches;

public static class MatchEndText
{
    private const string EloTransitionFormat = "{0} » {1}";
    private const string EloChangeFormat = "+0;-0;0";
    private const string PlayedTimeFormat = "{0}:{1:00}";
    private const int SecondsPerMinute = 60;
    private const int OneWall = 1;
    private const int FourPlayerCount = 4;
    private const string NoPlacement = "-";

    public static string DescribeTitle(PlayerStanding ownStanding, int playerCount)
    {
        ArgumentNullException.ThrowIfNull(ownStanding);

        if (playerCount == FourPlayerCount)
        {
            return DescribePlacement(ownStanding);
        }

        switch (ownStanding.Result)
        {
            case StandingResult.Won:
                return Strings.MatchEndVictoryTitle;
            case StandingResult.Lost:
                return Strings.MatchEndDefeatTitle;
            case StandingResult.Draw:
                return Strings.MatchEndDrawTitle;
            default:
                return Strings.MatchOverTitle;
        }
    }

    public static string DescribeOwnReason(EndedMatchReason endReason)
    {
        switch (endReason)
        {
            case EndedMatchReason.Goal:
                return Strings.MatchEndOwnGoal;
            case EndedMatchReason.Timeout:
                return Strings.MatchEndOwnTimeout;
            case EndedMatchReason.Resignation:
                return Strings.MatchEndOwnResignation;
            case EndedMatchReason.Draw:
                return Strings.MatchEndAgreedDraw;
            case EndedMatchReason.Abandonment:
                return Strings.MatchEndOwnAbandonment;
            default:
                throw new ArgumentOutOfRangeException(nameof(endReason), endReason, "Unsupported match end reason.");
        }
    }

    public static string DescribeRivalReason(EndedMatchReason endReason, string rivalNickname)
    {
        ArgumentNullException.ThrowIfNull(rivalNickname);

        string reasonFormat = FindRivalReasonFormat(endReason);
        return string.Format(CultureInfo.CurrentCulture, reasonFormat, rivalNickname);
    }

    public static string DescribeMatchGoesOn(int activePlayerCount)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.MatchEndGoesOnFormat, activePlayerCount);
    }

    public static string DescribeResult(PlayerStanding standing, bool hasAbandoned)
    {
        ArgumentNullException.ThrowIfNull(standing);

        if (standing.Result == StandingResult.StillPlaying)
        {
            return Strings.MatchEndResultPlaying;
        }

        bool hasLeft = hasAbandoned || standing.EndReason == EndedMatchReason.Abandonment;
        if (hasLeft)
        {
            return Strings.MatchEndResultLeft;
        }

        if (standing.EndReason == EndedMatchReason.Goal)
        {
            return Strings.MatchEndResultGoal;
        }

        return DescribeFinalResult(standing.Result);
    }

    public static string FormatPlace(PlayerStanding standing, int playerCount)
    {
        ArgumentNullException.ThrowIfNull(standing);

        if (playerCount != FourPlayerCount)
        {
            return string.Empty;
        }

        if (standing.Placement is byte placement)
        {
            return placement.ToString(CultureInfo.CurrentCulture);
        }

        return NoPlacement;
    }

    public static string FormatEloTransition(short initialElo, short finalElo)
    {
        return string.Format(CultureInfo.CurrentCulture, EloTransitionFormat, initialElo, finalElo);
    }

    public static string FormatEloChange(int eloChange)
    {
        return eloChange.ToString(EloChangeFormat, CultureInfo.CurrentCulture);
    }

    public static string FormatSummary(int moveCount, int playedSeconds, byte remainingWalls)
    {
        int minutes = playedSeconds / SecondsPerMinute;
        int seconds = playedSeconds % SecondsPerMinute;
        string playedTime = string.Format(CultureInfo.InvariantCulture, PlayedTimeFormat, minutes, seconds);
        string summaryFormat = remainingWalls == OneWall
            ? Strings.MatchEndSummaryOneWallFormat
            : Strings.MatchEndSummaryFormat;
        return string.Format(CultureInfo.CurrentCulture, summaryFormat, moveCount, playedTime, remainingWalls);
    }

    private static string DescribePlacement(PlayerStanding ownStanding)
    {
        if (ownStanding.Placement is not byte placement)
        {
            return Strings.MatchOverTitle;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchEndPlacementFormat, placement);
    }

    private static string FindRivalReasonFormat(EndedMatchReason endReason)
    {
        switch (endReason)
        {
            case EndedMatchReason.Goal:
                return Strings.MatchEndRivalGoalFormat;
            case EndedMatchReason.Timeout:
                return Strings.MatchEndRivalTimeoutFormat;
            case EndedMatchReason.Resignation:
                return Strings.MatchEndRivalResignationFormat;
            case EndedMatchReason.Draw:
                return Strings.MatchEndAgreedDraw;
            case EndedMatchReason.Abandonment:
                return Strings.MatchEndRivalAbandonmentFormat;
            default:
                throw new ArgumentOutOfRangeException(nameof(endReason), endReason, "Unsupported match end reason.");
        }
    }

    private static string DescribeFinalResult(StandingResult result)
    {
        switch (result)
        {
            case StandingResult.Won:
                return Strings.MatchEndResultWon;
            case StandingResult.Lost:
                return Strings.MatchEndResultLost;
            case StandingResult.Draw:
                return Strings.MatchEndResultDraw;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported final result.");
        }
    }
}
