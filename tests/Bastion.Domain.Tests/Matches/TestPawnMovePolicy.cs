using System.Collections.Generic;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestPawnMovePolicy
{
    private const byte ClassicBoardSize = 9;
    private const int OpenBoardReachableCount = 4;
    private const int CornerReachableCount = 2;
    private const int FirstIndex = 0;
    private const int LeftIndex = 3;
    private const int CenterIndex = 4;
    private const int AheadIndex = 5;
    private const int TwoAheadIndex = 6;
    private const int BeforeLastIndex = 7;
    private const int LastIndex = 8;

    private static readonly BoardCoordinates _corner = new BoardCoordinates(FirstIndex, FirstIndex);
    private static readonly BoardCoordinates _center = new BoardCoordinates(CenterIndex, CenterIndex);
    private static readonly BoardCoordinates _aheadOfCenter = new BoardCoordinates(CenterIndex, AheadIndex);
    private static readonly BoardCoordinates _twoAheadOfCenter = new BoardCoordinates(CenterIndex, TwoAheadIndex);
    private static readonly BoardCoordinates _leftOfAhead = new BoardCoordinates(LeftIndex, AheadIndex);
    private static readonly BoardCoordinates _beforeLastRow = new BoardCoordinates(CenterIndex, BeforeLastIndex);
    private static readonly BoardCoordinates _lastRow = new BoardCoordinates(CenterIndex, LastIndex);
    private static readonly BoardCoordinates _leftOfLastRow = new BoardCoordinates(LeftIndex, LastIndex);

    [Fact]
    public void ListReachableSquares_PawnInOpenBoard_ReturnsFourNeighbors()
    {
        Board board = CreateBoard(_center, _corner);

        IReadOnlyList<BoardCoordinates> reachableSquares = ListFirstPlayerSquares(board);

        Assert.Equal(OpenBoardReachableCount, reachableSquares.Count);
    }

    [Fact]
    public void ListReachableSquares_PawnInCorner_ReturnsTwoNeighbors()
    {
        Board board = CreateBoard(_corner, _center);

        IReadOnlyList<BoardCoordinates> reachableSquares = ListFirstPlayerSquares(board);

        Assert.Equal(CornerReachableCount, reachableSquares.Count);
    }

    [Fact]
    public void ListReachableSquares_RivalAhead_IncludesStraightJump()
    {
        Board board = CreateBoard(_center, _aheadOfCenter);

        IReadOnlyList<BoardCoordinates> reachableSquares = ListFirstPlayerSquares(board);

        Assert.Contains(_twoAheadOfCenter, reachableSquares);
    }

    [Fact]
    public void ListReachableSquares_RivalAhead_ExcludesRivalSquare()
    {
        Board board = CreateBoard(_center, _aheadOfCenter);

        IReadOnlyList<BoardCoordinates> reachableSquares = ListFirstPlayerSquares(board);

        Assert.DoesNotContain(_aheadOfCenter, reachableSquares);
    }

    [Fact]
    public void ListReachableSquares_RivalAgainstEdge_IncludesDiagonalJump()
    {
        Board board = CreateBoard(_beforeLastRow, _lastRow);

        IReadOnlyList<BoardCoordinates> reachableSquares = ListFirstPlayerSquares(board);

        Assert.Contains(_leftOfLastRow, reachableSquares);
    }

    [Fact]
    public void ListReachableSquares_PawnBehindRival_IncludesDiagonalJump()
    {
        var pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>
        {
            [Match.FirstTurnOrder] = _center,
            [Match.SecondTurnOrder] = _aheadOfCenter,
            [Match.ThirdTurnOrder] = _twoAheadOfCenter,
        };
        var board = new Board(ClassicBoardSize, pawnsByTurnOrder);

        IReadOnlyList<BoardCoordinates> reachableSquares = ListFirstPlayerSquares(board);

        Assert.Contains(_leftOfAhead, reachableSquares);
    }

    private static Board CreateBoard(BoardCoordinates firstPawn, BoardCoordinates secondPawn)
    {
        var pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>
        {
            [Match.FirstTurnOrder] = firstPawn,
            [Match.SecondTurnOrder] = secondPawn,
        };
        return new Board(ClassicBoardSize, pawnsByTurnOrder);
    }

    private static IReadOnlyList<BoardCoordinates> ListFirstPlayerSquares(Board board)
    {
        return PawnMovePolicy.ListReachableSquares(board, Match.FirstTurnOrder);
    }
}
