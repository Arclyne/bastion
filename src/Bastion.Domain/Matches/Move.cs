using System;

namespace Bastion.Domain.Matches;

public class Move
{
    internal Move(PawnMove pawnMove, string fromSquare, MoveTiming timing)
    {
        MoveNumber = pawnMove.MoveNumber;
        AccountId = pawnMove.AccountId;
        MoveType = MoveType.PawnMove;
        FromSquare = fromSquare;
        ToSquare = BoardLayout.ToSquare(pawnMove.Destination);
        TimeSpentMilliseconds = timing.TimeSpentMilliseconds;
        PlayedAt = timing.PlayedAt;
    }

    private Move()
    {
    }

    public int MatchId { get; private set; }

    public short MoveNumber { get; private set; }

    public int AccountId { get; private set; }

    public MoveType MoveType { get; private set; }

    public string? FromSquare { get; private set; }

    public string? ToSquare { get; private set; }

    public int TimeSpentMilliseconds { get; private set; }

    public DateTime PlayedAt { get; private set; }

    public BoardCoordinates GetDestination()
    {
        if (MoveType != MoveType.PawnMove || ToSquare is null)
        {
            throw new InvalidOperationException("Only a pawn move has a destination square.");
        }

        BoardCoordinates destination = BoardLayout.ParseSquare(ToSquare);
        return destination;
    }
}
