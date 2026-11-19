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
        if (!board.IsStepOpen(pawn, step))
        {
            return [];
        }

        BoardCoordinates neighbor = step.Advance(pawn);
        if (!board.IsOccupied(neighbor))
        {
            return [neighbor];
        }

        if (CanStepFrom(board, neighbor, step))
        {
            BoardCoordinates straightLanding = step.Advance(neighbor);
            return [straightLanding];
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
            if (CanStepFrom(board, jumpedPawn, side))
            {
                BoardCoordinates landing = side.Advance(jumpedPawn);
                diagonalLandings.Add(landing);
            }
        }

        return diagonalLandings;
    }

    private static bool CanStepFrom(Board board, BoardCoordinates from, BoardStep step)
    {
        BoardCoordinates landing = step.Advance(from);
        return board.IsStepOpen(from, step) && !board.IsOccupied(landing);
    }
}
