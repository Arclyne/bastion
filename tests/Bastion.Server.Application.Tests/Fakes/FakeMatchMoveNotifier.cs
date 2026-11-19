using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchMoveNotifier : IMatchMoveNotifier
{
    public FakeMatchMoveNotifier()
    {
        NotifiedPawnMoves = [];
        NotifiedWallPlacements = [];
    }

    public List<NotifiedPawnMove> NotifiedPawnMoves { get; }

    public List<NotifiedWallPlacement> NotifiedWallPlacements { get; }

    public void NotifyPawnMoved(int matchId, IReadOnlyCollection<int> recipientAccountIds, PawnMoveNotice notice)
    {
        var notifiedPawnMove = new NotifiedPawnMove(matchId, recipientAccountIds, notice);
        NotifiedPawnMoves.Add(notifiedPawnMove);
    }

    public void NotifyWallPlaced(int matchId, IReadOnlyCollection<int> recipientAccountIds, WallPlacementNotice notice)
    {
        var notifiedWallPlacement = new NotifiedWallPlacement(matchId, recipientAccountIds, notice);
        NotifiedWallPlacements.Add(notifiedWallPlacement);
    }
}
