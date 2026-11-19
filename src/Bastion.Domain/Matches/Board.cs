using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public sealed record Board
{
    private readonly Dictionary<byte, BoardCoordinates> _pawnsByTurnOrder;
    private readonly HashSet<Wall> _walls;

    public Board(
        byte size,
        IReadOnlyDictionary<byte, BoardCoordinates> pawnsByTurnOrder,
        IReadOnlyCollection<Wall> walls)
    {
        ArgumentNullException.ThrowIfNull(pawnsByTurnOrder);
        ArgumentNullException.ThrowIfNull(walls);

        Size = size;
        _pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>(pawnsByTurnOrder);
        _walls = [.. walls];
    }

    public byte Size { get; }

    public IReadOnlyCollection<Wall> Walls => _walls;

    public BoardCoordinates GetPawnCoordinates(byte turnOrder)
    {
        BoardCoordinates? pawn = _pawnsByTurnOrder.GetValueOrDefault(turnOrder);
        if (pawn is null)
        {
            throw new ArgumentOutOfRangeException(nameof(turnOrder), turnOrder, "No pawn plays that turn.");
        }

        return pawn;
    }

    public IReadOnlyList<byte> ListTurnOrders()
    {
        List<byte> turnOrders = [.. _pawnsByTurnOrder.Keys];
        return turnOrders;
    }

    public bool IsOccupied(BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        return _pawnsByTurnOrder.ContainsValue(coordinates);
    }

    public bool HasWall(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        return _walls.Contains(wall);
    }

    public bool IsStepOpen(BoardCoordinates from, BoardStep step)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(step);

        BoardCoordinates destination = step.Advance(from);
        return IsInside(destination) && !IsBlocked(from, step);
    }

    public Board CopyWithWall(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        var walls = new List<Wall>(_walls) { wall };
        return new Board(Size, _pawnsByTurnOrder, walls);
    }

    private bool IsInside(BoardCoordinates coordinates)
    {
        return BoardLayout.IsInside(Size, coordinates);
    }

    private bool IsBlocked(BoardCoordinates from, BoardStep step)
    {
        if (step.IsBetweenRows())
        {
            int lowerRowIndex = Math.Min(from.RowIndex, from.RowIndex + step.RowStep);
            return HasWallAlongRow(from.ColumnIndex, lowerRowIndex);
        }

        int leftColumnIndex = Math.Min(from.ColumnIndex, from.ColumnIndex + step.ColumnStep);
        return HasWallAlongColumn(leftColumnIndex, from.RowIndex);
    }

    private bool HasWallAlongRow(int columnIndex, int lowerRowIndex)
    {
        int previousColumnIndex = columnIndex - 1;
        var sameColumnGroove = new BoardCoordinates(columnIndex, lowerRowIndex);
        var previousColumnGroove = new BoardCoordinates(previousColumnIndex, lowerRowIndex);
        var sameColumnWall = new Wall(sameColumnGroove, WallOrientation.Horizontal);
        var previousColumnWall = new Wall(previousColumnGroove, WallOrientation.Horizontal);
        return _walls.Contains(sameColumnWall) || _walls.Contains(previousColumnWall);
    }

    private bool HasWallAlongColumn(int leftColumnIndex, int rowIndex)
    {
        int previousRowIndex = rowIndex - 1;
        var sameRowGroove = new BoardCoordinates(leftColumnIndex, rowIndex);
        var previousRowGroove = new BoardCoordinates(leftColumnIndex, previousRowIndex);
        var sameRowWall = new Wall(sameRowGroove, WallOrientation.Vertical);
        var previousRowWall = new Wall(previousRowGroove, WallOrientation.Vertical);
        return _walls.Contains(sameRowWall) || _walls.Contains(previousRowWall);
    }
}
