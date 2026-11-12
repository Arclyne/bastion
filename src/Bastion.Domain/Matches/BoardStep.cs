using System;
using System.Collections.Generic;

namespace Bastion.Domain.Matches;

public sealed record BoardStep(int ColumnStep, int RowStep)
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

    public static IReadOnlyList<BoardStep> OrthogonalSteps => _orthogonalSteps;

    public BoardCoordinates Advance(BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        int columnIndex = coordinates.ColumnIndex + ColumnStep;
        int rowIndex = coordinates.RowIndex + RowStep;
        return new BoardCoordinates(columnIndex, rowIndex);
    }
}
