using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public sealed record Board
{
    private const int FirstIndex = 0;
    private const int PreviousIndexOffset = 1;

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
        _walls = new HashSet<Wall>(walls);
    }

    public byte Size { get; }

    public IReadOnlyCollection<Wall> Walls => _walls;

    public BoardCoordinates GetPawnCoordinates(byte turnOrder)
    {
        if (!_pawnsByTurnOrder.TryGetValue(turnOrder, out BoardCoordinates? pawn))
        {
            throw new ArgumentOutOfRangeException(nameof(turnOrder), turnOrder, "No pawn plays that turn.");
        }

        return pawn;
    }

    public IReadOnlyList<byte> ListTurnOrders()
    {
        var turnOrders = new List<byte>(_pawnsByTurnOrder.Keys);
        return turnOrders;
    }

    public bool IsInside(BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        bool isColumnInside = coordinates.ColumnIndex >= FirstIndex && coordinates.ColumnIndex < Size;
        bool isRowInside = coordinates.RowIndex >= FirstIndex && coordinates.RowIndex < Size;
        return isColumnInside && isRowInside;
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

    public bool IsBlocked(BoardCoordinates from, BoardStep step)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(step);

        if (step.RowStep != 0)
        {
            int lowerRowIndex = Math.Min(from.RowIndex, from.RowIndex + step.RowStep);
            return HasWallAlongRow(from.ColumnIndex, lowerRowIndex);
        }

        int leftColumnIndex = Math.Min(from.ColumnIndex, from.ColumnIndex + step.ColumnStep);
        return HasWallAlongColumn(leftColumnIndex, from.RowIndex);
    }

    public Board WithWall(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        var walls = new List<Wall>(_walls) { wall };
        return new Board(Size, _pawnsByTurnOrder, walls);
    }

    private bool HasWallAlongRow(int columnIndex, int lowerRowIndex)
    {
        int previousColumnIndex = columnIndex - PreviousIndexOffset;
        var sameColumnGroove = new BoardCoordinates(columnIndex, lowerRowIndex);
        var previousColumnGroove = new BoardCoordinates(previousColumnIndex, lowerRowIndex);
        var sameColumnWall = new Wall(sameColumnGroove, WallOrientation.Horizontal);
        var previousColumnWall = new Wall(previousColumnGroove, WallOrientation.Horizontal);
        return _walls.Contains(sameColumnWall) || _walls.Contains(previousColumnWall);
    }

    private bool HasWallAlongColumn(int leftColumnIndex, int rowIndex)
    {
        int previousRowIndex = rowIndex - PreviousIndexOffset;
        var sameRowGroove = new BoardCoordinates(leftColumnIndex, rowIndex);
        var previousRowGroove = new BoardCoordinates(leftColumnIndex, previousRowIndex);
        var sameRowWall = new Wall(sameRowGroove, WallOrientation.Vertical);
        var previousRowWall = new Wall(previousRowGroove, WallOrientation.Vertical);
        return _walls.Contains(sameRowWall) || _walls.Contains(previousRowWall);
    }
}
