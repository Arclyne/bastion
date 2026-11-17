using System;
using Bastion.Domain.Matches;
using Bastion.Domain.Tests.GameModes;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public sealed class TestBoardLayout
{
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

    private static readonly BoardCoordinates _classicFirstRowCenter = new BoardCoordinates(
        ClassicCenterColumnIndex,
        FirstRowIndex);
    private static readonly BoardCoordinates _firstSquare = new BoardCoordinates(FirstColumnIndex, FirstRowIndex);

    [Fact]
    public void GetStartingCoordinates_FirstTurnOrderOnClassicBoard_ReturnsCenterOfFirstRow()
    {
        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(
            GameModeSampleFactory.ClassicBoardSize,
            Match.FirstTurnOrder);

        Assert.Equal(_classicFirstRowCenter, coordinates);
    }

    [Fact]
    public void GetStartingCoordinates_SecondTurnOrderOnQuickBoard_ReturnsCenterOfLastRow()
    {
        var expectedCoordinates = new BoardCoordinates(QuickCenterColumnIndex, QuickLastRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(
            GameModeSampleFactory.QuickBoardSize,
            Match.SecondTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingCoordinates_ThirdTurnOrderOnClassicBoard_ReturnsCenterOfFirstColumn()
    {
        var expectedCoordinates = new BoardCoordinates(FirstColumnIndex, ClassicCenterRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(
            GameModeSampleFactory.ClassicBoardSize,
            Match.ThirdTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingCoordinates_FourthTurnOrderOnClassicBoard_ReturnsCenterOfLastColumn()
    {
        var expectedCoordinates = new BoardCoordinates(ClassicLastColumnIndex, ClassicCenterRowIndex);

        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(
            GameModeSampleFactory.ClassicBoardSize,
            Match.FourthTurnOrder);

        Assert.Equal(expectedCoordinates, coordinates);
    }

    [Fact]
    public void GetStartingSquare_ThirdTurnOrderOnClassicBoard_ReturnsA5()
    {
        string square = BoardLayout.GetStartingSquare(GameModeSampleFactory.ClassicBoardSize, Match.ThirdTurnOrder);

        Assert.Equal(ThirdPlayerSquare, square);
    }

    [Fact]
    public void ToSquare_CenterOfFirstRowOnClassicBoard_ReturnsE1()
    {
        string square = BoardLayout.ToSquare(_classicFirstRowCenter);

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
    public void FindSquare_TextOutsideNotation_ReturnsOffBoardSquare(string square)
    {
        BoardCoordinates coordinates = BoardLayout.FindSquare(square);

        Assert.Equal(BoardLayout.OffBoardSquare, coordinates);
    }

    [Fact]
    public void IsGoal_FirstTurnOrderOnLastRow_ReturnsTrue()
    {
        var coordinates = new BoardCoordinates(FirstColumnIndex, ClassicLastRowIndex);

        bool isGoal = BoardLayout.IsGoal(GameModeSampleFactory.ClassicBoardSize, Match.FirstTurnOrder, coordinates);

        Assert.True(isGoal);
    }

    [Fact]
    public void IsGoal_SecondTurnOrderOnFirstRow_ReturnsTrue()
    {
        bool isGoal = BoardLayout.IsGoal(GameModeSampleFactory.ClassicBoardSize, Match.SecondTurnOrder, _firstSquare);

        Assert.True(isGoal);
    }

    [Fact]
    public void IsGoal_FirstTurnOrderOnFirstRow_ReturnsFalse()
    {
        bool isGoal = BoardLayout.IsGoal(GameModeSampleFactory.ClassicBoardSize, Match.FirstTurnOrder, _firstSquare);

        Assert.False(isGoal);
    }

    [Fact]
    public void GetStartingCoordinates_FifthTurnOrder_ThrowsArgumentOutOfRangeException()
    {
        Action getCoordinates = () => BoardLayout.GetStartingCoordinates(
            GameModeSampleFactory.ClassicBoardSize,
            FifthTurnOrder);

        Assert.Throws<ArgumentOutOfRangeException>(getCoordinates);
    }
}
