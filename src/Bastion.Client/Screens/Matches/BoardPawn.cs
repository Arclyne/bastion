using System;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed record BoardPawn
{
    public BoardPawn(BoardCoordinates coordinates, PawnSymbol symbol)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        Coordinates = coordinates;
        Symbol = symbol;
    }

    public BoardCoordinates Coordinates { get; }

    public PawnSymbol Symbol { get; }
}
