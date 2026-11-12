using System;
using System.Globalization;

namespace Bastion.Domain.Matches;

public static class BoardLayout
{
    private const char FirstColumn = 'a';
    private const char LastColumn = 'z';
    private const int RowNumberOffset = 1;
    private const int FirstIndex = 0;
    private const int BoardHalves = 2;
    private const int ColumnLength = 1;
    private const int ShortestSquareLength = 2;

    public static BoardCoordinates GetStartingCoordinates(byte boardSize, byte turnOrder)
    {
        int centerIndex = boardSize / BoardHalves;
        int lastIndex = GetLastIndex(boardSize);
        switch (turnOrder)
        {
            case Match.FirstTurnOrder:
                return new BoardCoordinates(centerIndex, FirstIndex);
            case Match.SecondTurnOrder:
                return new BoardCoordinates(centerIndex, lastIndex);
            case Match.ThirdTurnOrder:
                return new BoardCoordinates(FirstIndex, centerIndex);
            case Match.FourthTurnOrder:
                return new BoardCoordinates(lastIndex, centerIndex);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(turnOrder),
                    turnOrder,
                    "Starting squares are defined for four players at most.");
        }
    }

    public static string GetStartingSquare(byte boardSize, byte turnOrder)
    {
        BoardCoordinates coordinates = GetStartingCoordinates(boardSize, turnOrder);
        string square = ToSquare(coordinates);
        return square;
    }

    public static string ToSquare(BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        int columnCode = FirstColumn + coordinates.ColumnIndex;
        char column = (char)columnCode;
        int rowNumber = coordinates.RowIndex + RowNumberOffset;
        return string.Create(CultureInfo.InvariantCulture, $"{column}{rowNumber}");
    }

    public static int GetLastIndex(byte boardSize)
    {
        int lastIndex = boardSize - RowNumberOffset;
        return lastIndex;
    }

    public static BoardCoordinates ParseSquare(string square)
    {
        ArgumentNullException.ThrowIfNull(square);

        if (!TryParseSquare(square, out BoardCoordinates coordinates))
        {
            throw new FormatException($"The square '{square}' is not in board notation.");
        }

        return coordinates;
    }

    public static bool TryParseSquare(string square, out BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(square);

        coordinates = new BoardCoordinates(FirstIndex, FirstIndex);
        if (square.Length < ShortestSquareLength)
        {
            return false;
        }

        char column = square[FirstIndex];
        ReadOnlySpan<char> rowText = square.AsSpan(ColumnLength);
        bool isRowNumber = int.TryParse(rowText, NumberStyles.None, CultureInfo.InvariantCulture, out int rowNumber);
        if (column < FirstColumn || column > LastColumn || !isRowNumber || rowNumber < RowNumberOffset)
        {
            return false;
        }

        int columnIndex = column - FirstColumn;
        int rowIndex = rowNumber - RowNumberOffset;
        coordinates = new BoardCoordinates(columnIndex, rowIndex);
        return true;
    }

    public static bool IsGoal(byte boardSize, byte turnOrder, BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        int lastIndex = boardSize - RowNumberOffset;
        switch (turnOrder)
        {
            case Match.FirstTurnOrder:
                return coordinates.RowIndex == lastIndex;
            case Match.SecondTurnOrder:
                return coordinates.RowIndex == FirstIndex;
            case Match.ThirdTurnOrder:
                return coordinates.ColumnIndex == lastIndex;
            case Match.FourthTurnOrder:
                return coordinates.ColumnIndex == FirstIndex;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(turnOrder),
                    turnOrder,
                    "Goals are defined for four players at most.");
        }
    }
}
