using System.Collections.Generic;
using Bastion.Server.Application.PrivateChat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePrivateMessageNotifier : IPrivateMessageNotifier
{
    public List<PrivateMessageDelivery> Deliveries { get; } = new List<PrivateMessageDelivery>();

    public void NotifyPrivateMessage(PrivateMessageDelivery delivery)
    {
        Deliveries.Add(delivery);
    }
}
