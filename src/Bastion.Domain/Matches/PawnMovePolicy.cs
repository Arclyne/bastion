using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public static class PawnMovePolicy
{
    public static IReadOnlyList<BoardCoordinates> ListReachableSquares(Board board, byte turnOrder)
    {
        ArgumentNullException.ThrowIfNull(board);

        BoardCoordinates pawn = board.GetPawnCoordinates(turnOrder);
        var reachableSquares = new List<BoardCoordinates>();
        foreach (BoardStep step in BoardStep.OrthogonalSteps)
        {
            List<BoardCoordinates> squaresInDirection = ListSquaresInDirection(board, pawn, step);
            reachableSquares.AddRange(squaresInDirection);
        }

        return reachableSquares;
    }

    private static List<BoardCoordinates> ListSquaresInDirection(Board board, BoardCoordinates pawn, BoardStep step)
    {
        BoardCoordinates neighbor = step.Advance(pawn);
        if (!board.IsInside(neighbor) || board.IsBlocked(pawn, step))
        {
            return new List<BoardCoordinates>();
        }

        if (!board.IsOccupied(neighbor))
        {
            return new List<BoardCoordinates> { neighbor };
        }

        BoardCoordinates straightLanding = step.Advance(neighbor);
        if (CanStepTo(board, neighbor, step))
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
        BoardStep secondSide = firstSide.GetOpposite();
        BoardStep[] sides = { firstSide, secondSide };
        var diagonalLandings = new List<BoardCoordinates>();
        foreach (BoardStep side in sides)
        {
            if (CanStepTo(board, jumpedPawn, side))
            {
                BoardCoordinates landing = side.Advance(jumpedPawn);
                diagonalLandings.Add(landing);
            }
        }

        return diagonalLandings;
    }

    private static bool CanStepTo(Board board, BoardCoordinates from, BoardStep step)
    {
        BoardCoordinates landing = step.Advance(from);
        return board.IsInside(landing) && !board.IsBlocked(from, step) && !board.IsOccupied(landing);
    }
}
