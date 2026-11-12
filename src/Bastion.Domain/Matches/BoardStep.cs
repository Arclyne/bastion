using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public sealed record BoardStep(int ColumnStep, int RowStep)
{
    private const int Forward = 1;
    private const int Backward = -1;
    private const int Still = 0;

    private static readonly BoardStep _nextRow = new BoardStep(Still, Forward);
    private static readonly BoardStep _previousRow = new BoardStep(Still, Backward);
    private static readonly BoardStep _nextColumn = new BoardStep(Forward, Still);
    private static readonly BoardStep _previousColumn = new BoardStep(Backward, Still);
    private static readonly BoardStep[] _orthogonalSteps =
    {
        _nextRow,
        _previousRow,
        _nextColumn,
        _previousColumn,
    };

    public static BoardStep NextRow => _nextRow;

    public static BoardStep PreviousRow => _previousRow;

    public static BoardStep NextColumn => _nextColumn;

    public static BoardStep PreviousColumn => _previousColumn;

    public static IReadOnlyList<BoardStep> OrthogonalSteps => _orthogonalSteps;

    public BoardCoordinates Advance(BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        int columnIndex = coordinates.ColumnIndex + ColumnStep;
        int rowIndex = coordinates.RowIndex + RowStep;
        return new BoardCoordinates(columnIndex, rowIndex);
    }

    public BoardStep GetOpposite()
    {
        int columnStep = ColumnStep * Backward;
        int rowStep = RowStep * Backward;
        return new BoardStep(columnStep, rowStep);
    }

    public bool IsBetweenRows()
    {
        return RowStep != Still;
    }
}
