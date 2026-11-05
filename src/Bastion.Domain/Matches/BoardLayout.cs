using System;
using System.Globalization;

namespace Bastion.Domain.Matches;

public static class BoardLayout
{
    private const char FirstColumn = 'a';
    private const int FirstRow = 1;
    private const int BoardHalves = 2;

    public static string GetStartingSquare(byte boardSize, byte turnOrder)
    {
        char centerColumn = (char)(FirstColumn + (boardSize / BoardHalves));
        int startingRow = GetStartingRow(boardSize, turnOrder);
        return string.Create(CultureInfo.InvariantCulture, $"{centerColumn}{startingRow}");
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
