using System.Collections.Generic;

namespace Bastion.Server.Application.Matches;

public interface IMatchConnectionNotifier
{
    void NotifyOpponentDisconnected(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        OpponentDisconnection disconnection);

    void NotifyOpponentReconnected(int matchId, IReadOnlyCollection<int> recipientAccountIds, byte turnOrder);
}
