using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchEndNotifier : IMatchEndNotifier
{
    public FakeMatchEndNotifier()
    {
        NotifiedParticipationEnds = [];
        NotifiedMatchEnds = [];
    }

    public List<NotifiedParticipationEnd> NotifiedParticipationEnds { get; }

    public List<NotifiedMatchEnd> NotifiedMatchEnds { get; }

    public void NotifyParticipationEnded(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        ParticipationEndNotice notice)
    {
        var notifiedEnd = new NotifiedParticipationEnd(matchId, recipientAccountIds, notice);
        NotifiedParticipationEnds.Add(notifiedEnd);
    }

    public void NotifyMatchEnded(int matchId, IReadOnlyCollection<int> recipientAccountIds, MatchEndNotice notice)
    {
        var notifiedEnd = new NotifiedMatchEnd(matchId, recipientAccountIds, notice);
        NotifiedMatchEnds.Add(notifiedEnd);
    }
}
