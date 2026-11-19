using System;
using Bastion.Domain.Matches;

namespace Bastion.Domain.Statistics;

public class ModeStatistic
{
    public const short DefaultEloRating = 1000;

    private ModeStatistic(int accountId, byte gameModeId)
    {
        AccountId = accountId;
        GameModeId = gameModeId;
        EloRating = DefaultEloRating;
    }

    private ModeStatistic()
    {
    }

    public int AccountId { get; private set; }

    public byte GameModeId { get; private set; }

    public short EloRating { get; private set; }

    public DateTime? LastMatchAt { get; private set; }

    public int MatchesPlayed { get; private set; }

    public int MatchesWon { get; private set; }

    public int MatchesLost { get; private set; }

    public int Abandonments { get; private set; }

    public static ModeStatistic StartFor(int accountId, byte gameModeId)
    {
        return new ModeStatistic(accountId, gameModeId);
    }

    public void RecordMatch(ParticipationResult result, short finalElo, DateTime endedAt)
    {
        MatchesPlayed++;
        EloRating = finalElo;
        LastMatchAt = endedAt;
        if (result == ParticipationResult.Won)
        {
            MatchesWon++;
            return;
        }

        if (result == ParticipationResult.Lost)
        {
            MatchesLost++;
        }
    }

    public void RecordAbandonment()
    {
        Abandonments++;
    }
}
