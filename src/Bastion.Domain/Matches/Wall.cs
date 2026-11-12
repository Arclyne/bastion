using System;

namespace Bastion.Domain.Matches;

public sealed record Wall
{
    public Wall(BoardCoordinates groove, WallOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(groove);

        Groove = groove;
        Orientation = orientation;
    }

    public BoardCoordinates Groove { get; }

    public WallOrientation Orientation { get; }
}
