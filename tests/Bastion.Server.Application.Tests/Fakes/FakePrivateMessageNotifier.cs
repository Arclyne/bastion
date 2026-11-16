using System.Collections.Generic;
using Bastion.Server.Application.PrivateChat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePrivateMessageNotifier : IPrivateMessageNotifier
{
    public FakePrivateMessageNotifier()
    {
        Deliveries = new List<PrivateMessageDelivery>();
    }

    public List<PrivateMessageDelivery> Deliveries { get; }

    public void NotifyPrivateMessage(PrivateMessageDelivery delivery)
    {
        Deliveries.Add(delivery);
    }
}
