using System;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;
using Xunit;

namespace Bastion.Domain.Tests.Statistics;

public sealed class TestModeStatistic
{
    private const int AccountId = 4;
    private const byte GameModeId = 1;
    private const short FinalElo = 1018;
    private const int OneMatch = 1;
    private const int NoMatches = 0;

    private static readonly DateTime _endedAt = new DateTime(2026, 11, 19, 20, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void StartFor_NewAccount_StartsWithDefaultElo()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        Assert.Equal(ModeStatistic.DefaultEloRating, statistic.EloRating);
    }

    [Fact]
    public void RecordMatch_Win_CountsPlayedMatch()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordMatch(ParticipationResult.Won, FinalElo, _endedAt);

        Assert.Equal(OneMatch, statistic.MatchesPlayed);
    }

    [Fact]
    public void RecordMatch_Win_CountsWonMatch()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordMatch(ParticipationResult.Won, FinalElo, _endedAt);

        Assert.Equal(OneMatch, statistic.MatchesWon);
    }

    [Fact]
    public void RecordMatch_Loss_CountsLostMatch()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordMatch(ParticipationResult.Lost, FinalElo, _endedAt);

        Assert.Equal(OneMatch, statistic.MatchesLost);
    }

    [Fact]
    public void RecordMatch_Draw_CountsNeitherWinNorLoss()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordMatch(ParticipationResult.Draw, FinalElo, _endedAt);

        (int Played, int Won, int Lost) counters =
            (statistic.MatchesPlayed, statistic.MatchesWon, statistic.MatchesLost);
        Assert.Equal((OneMatch, NoMatches, NoMatches), counters);
    }

    [Fact]
    public void RecordMatch_AnyResult_KeepsFinalElo()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordMatch(ParticipationResult.Lost, FinalElo, _endedAt);

        Assert.Equal(FinalElo, statistic.EloRating);
    }

    [Fact]
    public void RecordMatch_AnyResult_KeepsEndTime()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordMatch(ParticipationResult.Won, FinalElo, _endedAt);

        Assert.Equal(_endedAt, statistic.LastMatchAt);
    }

    [Fact]
    public void RecordAbandonment_FirstAbandonment_CountsIt()
    {
        var statistic = ModeStatistic.StartFor(AccountId, GameModeId);

        statistic.RecordAbandonment();

        Assert.Equal(OneMatch, statistic.Abandonments);
    }
}
