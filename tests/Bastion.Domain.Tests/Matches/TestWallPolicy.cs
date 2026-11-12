using System.Collections.Generic;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public sealed class TestWallPolicy
{
    private const byte ClassicBoardSize = 9;
    private const int FirstIndex = 0;
    private const int SecondIndex = 1;
    private const int ThirdIndex = 2;
    private const int FourthIndex = 3;
    private const int CenterIndex = 4;
    private const int RightOfCenterIndex = 5;
    private const int TwoRightOfCenterIndex = 6;
    private const int LastIndex = 8;

    private static readonly BoardCoordinates _firstStart = new BoardCoordinates(CenterIndex, FirstIndex);
    private static readonly BoardCoordinates _secondStart = new BoardCoordinates(CenterIndex, LastIndex);
    private static readonly BoardCoordinates _corner = new BoardCoordinates(FirstIndex, FirstIndex);
    private static readonly BoardCoordinates _centerGroove = new BoardCoordinates(CenterIndex, FourthIndex);
    private static readonly BoardCoordinates _rightOfCenterGroove =
        new BoardCoordinates(RightOfCenterIndex, FourthIndex);
    private static readonly BoardCoordinates _twoRightOfCenterGroove =
        new BoardCoordinates(TwoRightOfCenterIndex, FourthIndex);
    private static readonly BoardCoordinates _lowColumnGroove = new BoardCoordinates(ThirdIndex, ThirdIndex);
    private static readonly BoardCoordinates _highColumnGroove = new BoardCoordinates(ThirdIndex, FourthIndex);
    private static readonly BoardCoordinates _lastColumnGroove = new BoardCoordinates(LastIndex, FirstIndex);
    private static readonly BoardCoordinates _secondColumnGroove = new BoardCoordinates(SecondIndex, FirstIndex);

    private static readonly Wall _centerHorizontal = new Wall(_centerGroove, WallOrientation.Horizontal);

    [Fact]
    public void FindRejection_FreeGroove_ReturnsNull()
    {
        Board board = CreateBoard(_firstStart);

        WallRejection? rejection = WallPolicy.FindRejection(board, _centerHorizontal);

        Assert.Null(rejection);
    }

    [Fact]
    public void FindRejection_GrooveOnLastColumn_RejectsOutOfBoard()
    {
        Board board = CreateBoard(_firstStart);
        var wall = new Wall(_lastColumnGroove, WallOrientation.Horizontal);
        var expectedRejection = new WallRejection(WallRejectionReason.OutOfBoard, BlockedTurnOrder: null);

        WallRejection? rejection = WallPolicy.FindRejection(board, wall);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void FindRejection_SameWallPlaced_RejectsGrooveTaken()
    {
        Board board = CreateBoard(_firstStart, _centerHorizontal);
        var expectedRejection = new WallRejection(WallRejectionReason.GrooveTaken, BlockedTurnOrder: null);

        WallRejection? rejection = WallPolicy.FindRejection(board, _centerHorizontal);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void FindRejection_CrossingWallPlaced_RejectsGrooveTaken()
    {
        Board board = CreateBoard(_firstStart, _centerHorizontal);
        var crossingWall = new Wall(_centerGroove, WallOrientation.Vertical);
        var expectedRejection = new WallRejection(WallRejectionReason.GrooveTaken, BlockedTurnOrder: null);

        WallRejection? rejection = WallPolicy.FindRejection(board, crossingWall);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void FindRejection_HalfOverlappingWall_RejectsGrooveTaken()
    {
        Board board = CreateBoard(_firstStart, _centerHorizontal);
        var overlappingWall = new Wall(_rightOfCenterGroove, WallOrientation.Horizontal);
        var expectedRejection = new WallRejection(WallRejectionReason.GrooveTaken, BlockedTurnOrder: null);

        WallRejection? rejection = WallPolicy.FindRejection(board, overlappingWall);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void FindRejection_WallRightAfterPlacedOne_ReturnsNull()
    {
        Board board = CreateBoard(_firstStart, _centerHorizontal);
        var followingWall = new Wall(_twoRightOfCenterGroove, WallOrientation.Horizontal);

        WallRejection? rejection = WallPolicy.FindRejection(board, followingWall);

        Assert.Null(rejection);
    }

    [Fact]
    public void FindRejection_HalfOverlappingVerticalWall_RejectsGrooveTaken()
    {
        var placedWall = new Wall(_lowColumnGroove, WallOrientation.Vertical);
        Board board = CreateBoard(_firstStart, placedWall);
        var overlappingWall = new Wall(_highColumnGroove, WallOrientation.Vertical);
        var expectedRejection = new WallRejection(WallRejectionReason.GrooveTaken, BlockedTurnOrder: null);

        WallRejection? rejection = WallPolicy.FindRejection(board, overlappingWall);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void FindRejection_WallClosingCornerPocket_RejectsBlockingFirstPlayer()
    {
        var roofWall = new Wall(_corner, WallOrientation.Horizontal);
        Board board = CreateBoard(_corner, roofWall);
        var closingWall = new Wall(_secondColumnGroove, WallOrientation.Vertical);
        var expectedRejection = new WallRejection(WallRejectionReason.WouldBlockPlayer, Match.FirstTurnOrder);

        WallRejection? rejection = WallPolicy.FindRejection(board, closingWall);

        Assert.Equal(expectedRejection, rejection);
    }

    private static Board CreateBoard(BoardCoordinates firstPawn, params Wall[] walls)
    {
        var pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>
        {
            [Match.FirstTurnOrder] = firstPawn,
            [Match.SecondTurnOrder] = _secondStart,
        };
        return new Board(ClassicBoardSize, pawnsByTurnOrder, walls);
    }
}
