using System;
using System.Globalization;

namespace Bastion.Domain.Matches;

public static class BoardLayout
{
    private const char FirstColumn = 'a';
    private const int FirstRow = 1;
    private const int BoardHalves = 2;

    public static BoardCoordinates GetStartingCoordinates(byte boardSize, byte turnOrder)
    {
        int centerColumnIndex = boardSize / BoardHalves;
        int startingRowIndex = GetStartingRow(boardSize, turnOrder) - FirstRow;
        return new BoardCoordinates(centerColumnIndex, startingRowIndex);
    }

    public static string GetStartingSquare(byte boardSize, byte turnOrder)
    {
        BoardCoordinates coordinates = GetStartingCoordinates(boardSize, turnOrder);
        char column = (char)(FirstColumn + coordinates.ColumnIndex);
        return string.Create(CultureInfo.InvariantCulture, $"{column}{coordinates.RowIndex + FirstRow}");
    }

    private static int GetStartingRow(byte boardSize, byte turnOrder)
    {
        switch (turnOrder)
        {
            case Match.FirstTurnOrder:
                return FirstRow;
            case Match.SecondTurnOrder:
                return boardSize;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(turnOrder),
                    turnOrder,
                    "Starting squares are defined for two players only.");
        }
    }
}
