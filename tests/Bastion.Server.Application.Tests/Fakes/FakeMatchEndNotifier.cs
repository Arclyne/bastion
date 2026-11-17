using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchEndNotifier : IMatchEndNotifier
{
    public FakeMatchEndNotifier()
    {
        NotifiedAbandonments = new List<NotifiedMatchAbandonment>();
    }

    public List<NotifiedMatchAbandonment> NotifiedAbandonments { get; }

    public void NotifyMatchAbandoned(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        AbandonmentNotice notice)
    {
        var notifiedAbandonment = new NotifiedMatchAbandonment(matchId, recipientAccountIds, notice);
        NotifiedAbandonments.Add(notifiedAbandonment);
    }
}
