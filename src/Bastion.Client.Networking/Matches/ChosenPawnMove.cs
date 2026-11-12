using System;

namespace Bastion.Client.Networking.Matches;

public sealed record ChosenPawnMove
{
    public ChosenPawnMove(int matchId, short moveNumber, string toSquare)
    {
        ArgumentNullException.ThrowIfNull(toSquare);

        MatchId = matchId;
        MoveNumber = moveNumber;
        ToSquare = toSquare;
    }

    public int MatchId { get; }

    public short MoveNumber { get; }

    public string ToSquare { get; }
}
