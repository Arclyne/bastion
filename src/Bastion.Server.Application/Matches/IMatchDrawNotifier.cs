using System.Collections.Generic;

namespace Bastion.Server.Application.Matches;

public interface IMatchDrawNotifier
{
    void NotifyDrawOffered(int matchId, IReadOnlyCollection<int> recipientAccountIds, byte offererTurnOrder);

    void NotifyDrawRejected(int matchId, IReadOnlyCollection<int> recipientAccountIds);

    void NotifyDrawExpired(int matchId, IReadOnlyCollection<int> recipientAccountIds);
}
