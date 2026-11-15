using System;

namespace Bastion.Domain.Matches;

public class Move
{
    internal Move(PawnMove pawnMove, string fromSquare, MoveTiming timing)
        : this(pawnMove.MoveNumber, pawnMove.AccountId, timing)
    {
        MoveType = MoveType.PawnMove;
        FromSquare = fromSquare;
        ToSquare = BoardLayout.ToSquare(pawnMove.Destination);
    }

    internal Move(WallPlacement placement, MoveTiming timing)
        : this(placement.MoveNumber, placement.AccountId, timing)
    {
        MoveType = MoveType.Wall;
        Groove = BoardLayout.ToSquare(placement.Wall.Groove);
        Orientation = placement.Wall.Orientation;
    }

    private Move(short moveNumber, int accountId, MoveTiming timing)
    {
        MoveNumber = moveNumber;
        AccountId = accountId;
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

    public string? Groove { get; private set; }

    public WallOrientation? Orientation { get; private set; }

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

    public Wall GetWall()
    {
        if (MoveType != MoveType.Wall || Groove is null || Orientation is not WallOrientation orientation)
        {
            throw new InvalidOperationException("Only a wall move has a groove and an orientation.");
        }

        BoardCoordinates groove = BoardLayout.ParseSquare(Groove);
        return new Wall(groove, orientation);
    }
}
