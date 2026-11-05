using System;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestParticipation
{
    private const byte ZeroTurnOrder = 0;
    private const byte FifthTurnOrder = 5;

    [Fact]
    public void GetPawnSymbol_FirstTurnOrder_ReturnsBlue()
    {
        PawnSymbol symbol = Participation.GetPawnSymbol(Match.FirstTurnOrder);

        Assert.Equal(PawnSymbol.Blue, symbol);
    }

    [Fact]
    public void GetPawnSymbol_SecondTurnOrder_ReturnsRed()
    {
        PawnSymbol symbol = Participation.GetPawnSymbol(Match.SecondTurnOrder);

        Assert.Equal(PawnSymbol.Red, symbol);
    }

    [Fact]
    public void GetPawnSymbol_ZeroTurnOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Participation.GetPawnSymbol(ZeroTurnOrder));
    }

    [Fact]
    public void GetPawnSymbol_FifthTurnOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Participation.GetPawnSymbol(FifthTurnOrder));
    }
}
