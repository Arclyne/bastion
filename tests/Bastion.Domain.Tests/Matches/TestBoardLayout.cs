using System;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestBoardLayout
{
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const byte FifthTurnOrder = 5;
    private const int ClassicCenterColumnIndex = 4;
    private const int QuickCenterColumnIndex = 3;
    private const int FirstRowIndex = 0;
    private const int QuickLastRowIndex = 6;
    private const int ClassicCenterRowIndex = 4;
    private const int FirstColumnIndex = 0;
    private const int ClassicLastColumnIndex = 8;
    private const string ThirdPlayerSquare = "a5";

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
    public void GetStartingCoordinates_ThirdTurnOrderOnClassicBoard_ReturnsCenterOfFirstColumn()
    {
        var expectedCoordinates = new BoardCoordinates(FirstColumnIndex, ClassicCenterRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(ClassicBoardSize, Match.ThirdTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingCoordinates_FourthTurnOrderOnClassicBoard_ReturnsCenterOfLastColumn()
    {
        var expectedCoordinates = new BoardCoordinates(ClassicLastColumnIndex, ClassicCenterRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(ClassicBoardSize, Match.FourthTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingSquare_ThirdTurnOrderOnClassicBoard_ReturnsA5()
    {
        string square = BoardLayout.GetStartingSquare(ClassicBoardSize, Match.ThirdTurnOrder);

        Assert.Equal(ThirdPlayerSquare, square);
    }

    [Fact]
    public void GetStartingCoordinates_FifthTurnOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BoardLayout.GetStartingCoordinates(ClassicBoardSize, FifthTurnOrder));
    }
}
