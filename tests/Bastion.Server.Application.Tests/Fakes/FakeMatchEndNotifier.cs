using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchEndNotifier : IMatchEndNotifier
{
    public List<NotifiedMatchAbandonment> NotifiedAbandonments { get; } = new List<NotifiedMatchAbandonment>();

    public void NotifyMatchAbandoned(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        AbandonmentNotice notice)
    {
        var notifiedAbandonment = new NotifiedMatchAbandonment(matchId, recipientAccountIds, notice);
        NotifiedAbandonments.Add(notifiedAbandonment);
    }
}
