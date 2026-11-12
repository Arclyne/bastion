using System;
using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchMoveNotifier : IMatchMoveNotifier
{
    public List<NotifiedPawnMove> NotifiedPawnMoves { get; } = new List<NotifiedPawnMove>();

    public List<NotifiedWallPlacement> NotifiedWallPlacements { get; } = new List<NotifiedWallPlacement>();

    public void NotifyPawnMoved(int matchId, IReadOnlyCollection<int> recipientAccountIds, PawnMoveNotice notice)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(notice);

        var notifiedPawnMove = new NotifiedPawnMove(matchId, recipientAccountIds, notice);
        NotifiedPawnMoves.Add(notifiedPawnMove);
    }

    public void NotifyWallPlaced(int matchId, IReadOnlyCollection<int> recipientAccountIds, WallPlacementNotice notice)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(notice);

        NotifiedWallPlacements.Add(new NotifiedWallPlacement(matchId, recipientAccountIds, notice));
    }
}
