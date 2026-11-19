using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;
using Xunit;

namespace Bastion.Domain.Tests.Statistics;

public sealed class TestEloPolicy
{
    private const short EvenElo = 1500;
    private const short EvenWinnerFinalElo = 1516;
    private const short UnderdogElo = 1842;
    private const short FavoriteElo = 1886;
    private const short UnderdogWinnerFinalElo = 1860;
    private const short UnderdogLoserFinalElo = 1828;
    private const short UnderdogDrawFinalElo = 1844;
    private const short FavoriteDrawFinalElo = 1884;

    [Fact]
    public void CalculateFinalElo_EvenPlayersWin_AddsHalfTheFactor()
    {
        short finalElo = EloPolicy.CalculateFinalElo(EvenElo, EvenElo, ParticipationResult.Won);

        Assert.Equal(EvenWinnerFinalElo, finalElo);
    }

    [Fact]
    public void CalculateFinalElo_EvenPlayersDraw_KeepsElo()
    {
        short finalElo = EloPolicy.CalculateFinalElo(EvenElo, EvenElo, ParticipationResult.Draw);

        Assert.Equal(EvenElo, finalElo);
    }

    [Fact]
    public void CalculateFinalElo_UnderdogWins_AddsMoreThanHalfTheFactor()
    {
        short finalElo = EloPolicy.CalculateFinalElo(UnderdogElo, FavoriteElo, ParticipationResult.Won);

        Assert.Equal(UnderdogWinnerFinalElo, finalElo);
    }

    [Fact]
    public void CalculateFinalElo_UnderdogLoses_TakesLessThanHalfTheFactor()
    {
        short finalElo = EloPolicy.CalculateFinalElo(UnderdogElo, FavoriteElo, ParticipationResult.Lost);

        Assert.Equal(UnderdogLoserFinalElo, finalElo);
    }

    [Fact]
    public void CalculateFinalElo_UnderdogDraws_GainsElo()
    {
        short finalElo = EloPolicy.CalculateFinalElo(UnderdogElo, FavoriteElo, ParticipationResult.Draw);

        Assert.Equal(UnderdogDrawFinalElo, finalElo);
    }

    [Fact]
    public void CalculateFinalElo_FavoriteDraws_LosesElo()
    {
        short finalElo = EloPolicy.CalculateFinalElo(FavoriteElo, UnderdogElo, ParticipationResult.Draw);

        Assert.Equal(FavoriteDrawFinalElo, finalElo);
    }
}
