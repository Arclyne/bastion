using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public sealed record Board
{
    private const int FirstIndex = 0;

    private readonly Dictionary<byte, BoardCoordinates> _pawnsByTurnOrder;

    public Board(byte size, IReadOnlyDictionary<byte, BoardCoordinates> pawnsByTurnOrder)
    {
        ArgumentNullException.ThrowIfNull(pawnsByTurnOrder);

        Size = size;
        _pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>(pawnsByTurnOrder);
    }

    public byte Size { get; }

    public BoardCoordinates GetPawnCoordinates(byte turnOrder)
    {
        if (!_pawnsByTurnOrder.TryGetValue(turnOrder, out BoardCoordinates? pawn))
        {
            throw new ArgumentOutOfRangeException(nameof(turnOrder), turnOrder, "No pawn plays that turn.");
        }

        return pawn;
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
}
