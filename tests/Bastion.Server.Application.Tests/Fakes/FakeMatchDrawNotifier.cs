using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchDrawNotifier : IMatchDrawNotifier
{
    public FakeMatchDrawNotifier()
    {
        OffererTurnOrders = [];
        OfferRecipients = [];
        RejectionRecipients = [];
        ExpiryRecipients = [];
    }

    public List<byte> OffererTurnOrders { get; }

    public List<int> OfferRecipients { get; }

    public List<int> RejectionRecipients { get; }

    public List<int> ExpiryRecipients { get; }

    public void NotifyDrawOffered(int matchId, IReadOnlyCollection<int> recipientAccountIds, byte offererTurnOrder)
    {
        OffererTurnOrders.Add(offererTurnOrder);
        OfferRecipients.AddRange(recipientAccountIds);
    }

    public void NotifyDrawRejected(int matchId, IReadOnlyCollection<int> recipientAccountIds)
    {
        RejectionRecipients.AddRange(recipientAccountIds);
    }

    public void NotifyDrawExpired(int matchId, IReadOnlyCollection<int> recipientAccountIds)
    {
        ExpiryRecipients.AddRange(recipientAccountIds);
    }
}
