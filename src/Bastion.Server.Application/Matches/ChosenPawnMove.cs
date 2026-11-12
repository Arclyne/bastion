using System;

namespace Bastion.Server.Application.Matches;

public sealed record ChosenPawnMove
{
    public ChosenPawnMove(int matchId, short moveNumber, string destinationSquare)
    {
        ArgumentNullException.ThrowIfNull(destinationSquare);

        MatchId = matchId;
        MoveNumber = moveNumber;
        DestinationSquare = destinationSquare;
    }

    public int MatchId { get; }

    public short MoveNumber { get; }

    public string DestinationSquare { get; }
}
