using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public static class WallPolicy
{
    private const int FirstGrooveIndex = 0;
    private const int GrooveCountOffset = 1;
    private const int Forward = 1;
    private const int Backward = -1;
    private const int Still = 0;

    public static WallRejection? FindRejection(Board board, Wall wall)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(wall);

        if (!IsInside(board.Size, wall))
        {
            return new WallRejection(WallRejectionReason.OutOfBoard, null);
        }

        if (CollidesWithPlacedWall(board, wall))
        {
            return new WallRejection(WallRejectionReason.GrooveTaken, null);
        }

        Board boardWithWall = board.WithWall(wall);
        WallRejection? blockingRejection = FindBlockedPlayer(boardWithWall);
        return blockingRejection;
    }

    private static bool IsInside(byte boardSize, Wall wall)
    {
        int grooveCount = boardSize - GrooveCountOffset;
        BoardCoordinates groove = wall.Groove;
        bool isColumnInside = groove.ColumnIndex >= FirstGrooveIndex && groove.ColumnIndex < grooveCount;
        bool isRowInside = groove.RowIndex >= FirstGrooveIndex && groove.RowIndex < grooveCount;
        return isColumnInside && isRowInside;
    }

    private static bool CollidesWithPlacedWall(Board board, Wall wall)
    {
        WallOrientation crossingOrientation = wall.Orientation == WallOrientation.Horizontal
            ? WallOrientation.Vertical
            : WallOrientation.Horizontal;
        var crossingWall = new Wall(wall.Groove, crossingOrientation);
        if (board.HasWall(wall) || board.HasWall(crossingWall))
        {
            return true;
        }

        BoardStep alongWall = wall.Orientation == WallOrientation.Horizontal
            ? new BoardStep(Forward, Still)
            : new BoardStep(Still, Forward);
        var backAlongWall = new BoardStep(alongWall.ColumnStep * Backward, alongWall.RowStep * Backward);
        BoardCoordinates nextGroove = alongWall.Advance(wall.Groove);
        BoardCoordinates previousGroove = backAlongWall.Advance(wall.Groove);
        var nextWall = new Wall(nextGroove, wall.Orientation);
        var previousWall = new Wall(previousGroove, wall.Orientation);
        return board.HasWall(nextWall) || board.HasWall(previousWall);
    }

    private static WallRejection? FindBlockedPlayer(Board boardWithWall)
    {
        foreach (byte turnOrder in boardWithWall.ListTurnOrders())
        {
            if (!HasPathToGoal(boardWithWall, turnOrder))
            {
                return new WallRejection(WallRejectionReason.WouldBlockPlayer, turnOrder);
            }
        }

        return null;
    }

    private static bool HasPathToGoal(Board board, byte turnOrder)
    {
        BoardCoordinates start = board.GetPawn(turnOrder);
        var pendingSquares = new Queue<BoardCoordinates>();
        pendingSquares.Enqueue(start);
        var visitedSquares = new HashSet<BoardCoordinates> { start };
        while (pendingSquares.Count > 0)
        {
            BoardCoordinates square = pendingSquares.Dequeue();
            if (BoardLayout.IsGoal(board.Size, turnOrder, square))
            {
                return true;
            }

            foreach (BoardCoordinates neighbor in ListOpenNeighbors(board, square))
            {
                if (visitedSquares.Add(neighbor))
                {
                    pendingSquares.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    private static List<BoardCoordinates> ListOpenNeighbors(Board board, BoardCoordinates square)
    {
        var openNeighbors = new List<BoardCoordinates>();
        foreach (BoardStep step in BoardStep.OrthogonalSteps)
        {
            BoardCoordinates neighbor = step.Advance(square);
            if (board.IsInside(neighbor) && !board.IsBlocked(square, step))
            {
                openNeighbors.Add(neighbor);
            }
        }

        return openNeighbors;
    }
}
