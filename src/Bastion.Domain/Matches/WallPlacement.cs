using System;

namespace Bastion.Domain.Matches;

public sealed record WallPlacement
{
    public WallPlacement(int accountId, short moveNumber, Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        AccountId = accountId;
        MoveNumber = moveNumber;
        Wall = wall;
    }

    public int AccountId { get; }

    public short MoveNumber { get; }

    public Wall Wall { get; }
}
