using System;

namespace Bastion.Domain.Matches;

public sealed record PawnMove
{
    public PawnMove(int accountId, short moveNumber, BoardCoordinates destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        AccountId = accountId;
        MoveNumber = moveNumber;
        Destination = destination;
    }

    public int AccountId { get; }

    public short MoveNumber { get; }

    public BoardCoordinates Destination { get; }
}
