using System;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestBoardLayout
{
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const byte ThirdTurnOrder = 3;
    private const int ClassicCenterColumnIndex = 4;
    private const int QuickCenterColumnIndex = 3;
    private const int FirstRowIndex = 0;
    private const int QuickLastRowIndex = 6;

    [Fact]
    public void GetStartingCoordinates_FirstTurnOrderOnClassicBoard_ReturnsCenterOfFirstRow()
    {
        var expectedCoordinates = new BoardCoordinates(ClassicCenterColumnIndex, FirstRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(ClassicBoardSize, Match.FirstTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingCoordinates_SecondTurnOrderOnQuickBoard_ReturnsCenterOfLastRow()
    {
        var expectedCoordinates = new BoardCoordinates(QuickCenterColumnIndex, QuickLastRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(QuickBoardSize, Match.SecondTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingCoordinates_ThirdTurnOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BoardLayout.GetStartingCoordinates(ClassicBoardSize, ThirdTurnOrder));
    }
}
