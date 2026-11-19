using System.Collections.Generic;

namespace Bastion.Server.Application.Matches;

public interface IMatchEndNotifier
{
    void NotifyParticipationEnded(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        ParticipationEndNotice notice);

    void NotifyMatchEnded(int matchId, IReadOnlyCollection<int> recipientAccountIds, MatchEndNotice notice);
}
