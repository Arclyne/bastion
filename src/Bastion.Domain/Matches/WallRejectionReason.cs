namespace Bastion.Domain.Matches;

public enum WallRejectionReason
{
    None,
    NoWallsLeft,
    OutOfBoard,
    GrooveTaken,
    WouldBlockPlayer,
}
