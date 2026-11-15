using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public static class WallPolicy
{
    public static WallRejection? FindRejection(Board board, Wall wall)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(wall);

        if (!IsGrooveOnBoard(board.Size, wall.Groove))
        {
            return new WallRejection(WallRejectionReason.OutOfBoard, BlockedTurnOrder: null);
        }

        if (IsGrooveTaken(board, wall))
        {
            return new WallRejection(WallRejectionReason.GrooveTaken, BlockedTurnOrder: null);
        }

        Board boardWithWall = board.CopyWithWall(wall);
        WallRejection? blockingRejection = FindBlockedPlayer(boardWithWall);
        return blockingRejection;
    }

    public static bool IsGrooveOnBoard(byte boardSize, BoardCoordinates groove)
    {
        ArgumentNullException.ThrowIfNull(groove);

        int lastIndex = BoardLayout.GetLastIndex(boardSize);
        return BoardLayout.AreIndexesBelow(groove, lastIndex);
    }

    private static bool IsGrooveTaken(Board board, Wall wall)
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
            ? BoardStep.NextColumn
            : BoardStep.NextRow;
        BoardStep backAlongWall = alongWall.GetOpposite();
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
        BoardCoordinates start = board.GetPawnCoordinates(turnOrder);
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
            if (board.IsStepOpen(square, step))
            {
                BoardCoordinates neighbor = step.Advance(square);
                openNeighbors.Add(neighbor);
            }
        }

        return openNeighbors;
    }
}
