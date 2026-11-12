using System.Collections.Generic;

namespace Bastion.Server.Application.Matches;

public interface IMatchMoveNotifier
{
    void NotifyPawnMoved(int matchId, IReadOnlyCollection<int> recipientAccountIds, PawnMoveNotice notice);

    void NotifyWallPlaced(int matchId, IReadOnlyCollection<int> recipientAccountIds, WallPlacementNotice notice);
}
