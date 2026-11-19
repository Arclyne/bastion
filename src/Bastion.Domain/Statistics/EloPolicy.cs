using System;
using Bastion.Domain.Matches;

namespace Bastion.Domain.Statistics;

public static class EloPolicy
{
    public const int KFactor = 32;

    private const double RatingScale = 400;
    private const double ExponentBase = 10;
    private const double WinScore = 1;
    private const double DrawScore = 0.5;
    private const double LossScore = 0;

    public static short CalculateFinalElo(short ownElo, short rivalElo, ParticipationResult result)
    {
        double expectedScore = CalculateExpectedScore(ownElo, rivalElo);
        double actualScore = ToScore(result);
        double eloChange = KFactor * (actualScore - expectedScore);
        double roundedChange = Math.Round(eloChange, MidpointRounding.AwayFromZero);
        int wholeChange = (int)roundedChange;
        int finalEloNumber = ownElo + wholeChange;
        short finalElo = (short)finalEloNumber;
        return finalElo;
    }

    private static double CalculateExpectedScore(short ownElo, short rivalElo)
    {
        double ratingGap = (rivalElo - ownElo) / RatingScale;
        double rivalAdvantage = Math.Pow(ExponentBase, ratingGap);
        double expectedScore = WinScore / (WinScore + rivalAdvantage);
        return expectedScore;
    }

    private static double ToScore(ParticipationResult result)
    {
        switch (result)
        {
            case ParticipationResult.Won:
                return WinScore;
            case ParticipationResult.Draw:
                return DrawScore;
            case ParticipationResult.Lost:
                return LossScore;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, "The result has no elo score.");
        }
    }
}
