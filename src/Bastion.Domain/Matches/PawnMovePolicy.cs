using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public static class PawnMovePolicy
{
    private const int Forward = 1;
    private const int Backward = -1;
    private const int Still = 0;

    private static readonly BoardStep[] _orthogonalSteps = new BoardStep[]
    {
        new BoardStep(Still, Forward),
        new BoardStep(Still, Backward),
        new BoardStep(Forward, Still),
        new BoardStep(Backward, Still),
    };

    public static IReadOnlyList<BoardCoordinates> ListReachableSquares(Board board, byte turnOrder)
    {
        ArgumentNullException.ThrowIfNull(board);

        BoardCoordinates pawn = board.GetPawn(turnOrder);
        var reachableSquares = new List<BoardCoordinates>();
        foreach (BoardStep step in _orthogonalSteps)
        {
            List<BoardCoordinates> squaresInDirection = ListSquaresInDirection(board, pawn, step);
            reachableSquares.AddRange(squaresInDirection);
        }

        return reachableSquares;
    }

    private static List<BoardCoordinates> ListSquaresInDirection(Board board, BoardCoordinates pawn, BoardStep step)
    {
        BoardCoordinates neighbor = Advance(pawn, step);
        if (!board.IsInside(neighbor))
        {
            return new List<BoardCoordinates>();
        }

        if (!board.IsOccupied(neighbor))
        {
            return new List<BoardCoordinates> { neighbor };
        }

        BoardCoordinates straightLanding = Advance(neighbor, step);
        if (IsFree(board, straightLanding))
        {
            return new List<BoardCoordinates> { straightLanding };
        }

        List<BoardCoordinates> diagonalLandings = ListDiagonalLandings(board, neighbor, step);
        return diagonalLandings;
    }

    private static List<BoardCoordinates> ListDiagonalLandings(
        Board board,
        BoardCoordinates jumpedPawn,
        BoardStep step)
    {
        var firstSide = new BoardStep(step.RowStep, step.ColumnStep);
        var secondSide = new BoardStep(-step.RowStep, -step.ColumnStep);
        var diagonalLandings = new List<BoardCoordinates>();
        foreach (BoardStep side in new BoardStep[] { firstSide, secondSide })
        {
            BoardCoordinates landing = Advance(jumpedPawn, side);
            if (IsFree(board, landing))
            {
                diagonalLandings.Add(landing);
            }
        }

        return diagonalLandings;
    }

    private static bool IsFree(Board board, BoardCoordinates coordinates)
    {
        return board.IsInside(coordinates) && !board.IsOccupied(coordinates);
    }

    private static BoardCoordinates Advance(BoardCoordinates coordinates, BoardStep step)
    {
        int columnIndex = coordinates.ColumnIndex + step.ColumnStep;
        int rowIndex = coordinates.RowIndex + step.RowStep;
        return new BoardCoordinates(columnIndex, rowIndex);
    }
}
