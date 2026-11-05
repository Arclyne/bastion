using System.Collections.Generic;

namespace Bastion.Server.Application.Matches;

public interface IMatchEndNotifier
{
    void NotifyMatchAbandoned(int matchId, IReadOnlyCollection<int> recipientAccountIds, AbandonmentNotice notice);
}
