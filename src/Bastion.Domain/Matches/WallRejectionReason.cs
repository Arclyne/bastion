namespace Bastion.Domain.Matches;

public enum WallRejectionReason
{
    NoWallsLeft,
    OutOfBoard,
    GrooveTaken,
    WouldBlockPlayer,
}
