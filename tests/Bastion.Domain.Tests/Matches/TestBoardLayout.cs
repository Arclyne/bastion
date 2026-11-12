using System;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public sealed class TestBoardLayout
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
    private const string FirstPlayerSquare = "e1";
    private const string SecondPlayerSquare = "e9";
    private const string RowZeroSquare = "e0";
    private const int ClassicLastRowIndex = 8;

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
    public void ToSquare_CenterOfFirstRowOnClassicBoard_ReturnsE1()
    {
        var coordinates = new BoardCoordinates(ClassicCenterColumnIndex, FirstRowIndex);

        string square = BoardLayout.ToSquare(coordinates);

        Assert.Equal(FirstPlayerSquare, square);
    }

    [Fact]
    public void ParseSquare_E9_ReturnsCenterOfLastClassicRow()
    {
        var expectedCoordinates = new BoardCoordinates(ClassicCenterColumnIndex, ClassicLastRowIndex);

        BoardCoordinates coordinates = BoardLayout.ParseSquare(SecondPlayerSquare);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void ParseSquare_RowZero_ThrowsFormatException()
    {
        Action parse = () => BoardLayout.ParseSquare(RowZeroSquare);

        Assert.Throws<FormatException>(parse);
    }

    [Theory]
    [InlineData("e")]
    [InlineData("9e")]
    [InlineData("E1")]
    [InlineData("e-1")]
    public void TryParseSquare_TextOutsideNotation_ReturnsFalse(string square)
    {
        bool isSquare = BoardLayout.TryParseSquare(square, out _);

        Assert.False(isSquare);
    }

    [Fact]
    public void IsGoal_FirstTurnOrderOnLastRow_ReturnsTrue()
    {
        var coordinates = new BoardCoordinates(FirstColumnIndex, ClassicLastRowIndex);

        bool isGoal = BoardLayout.IsGoal(ClassicBoardSize, Match.FirstTurnOrder, coordinates);

        Assert.True(isGoal);
    }

    [Fact]
    public void IsGoal_SecondTurnOrderOnFirstRow_ReturnsTrue()
    {
        var coordinates = new BoardCoordinates(FirstColumnIndex, FirstRowIndex);

        bool isGoal = BoardLayout.IsGoal(ClassicBoardSize, Match.SecondTurnOrder, coordinates);

        Assert.True(isGoal);
    }

    [Fact]
    public void IsGoal_FirstTurnOrderOnFirstRow_ReturnsFalse()
    {
        var coordinates = new BoardCoordinates(FirstColumnIndex, FirstRowIndex);

        bool isGoal = BoardLayout.IsGoal(ClassicBoardSize, Match.FirstTurnOrder, coordinates);

        Assert.False(isGoal);
    }

    [Fact]
    public void GetStartingCoordinates_FifthTurnOrder_ThrowsArgumentOutOfRangeException()
    {
        Action getCoordinates = () => BoardLayout.GetStartingCoordinates(ClassicBoardSize, FifthTurnOrder);

        Assert.Throws<ArgumentOutOfRangeException>(getCoordinates);
    }
}
