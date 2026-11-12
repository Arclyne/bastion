using System;
using System.Globalization;

namespace Bastion.Domain.Matches;

public static class BoardLayout
{
    private const char FirstColumn = 'a';
    private const int RowNumberOffset = 1;
    private const int FirstIndex = 0;
    private const int BoardHalves = 2;

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
}
