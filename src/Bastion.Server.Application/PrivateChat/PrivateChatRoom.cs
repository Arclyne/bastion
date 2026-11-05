using System;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed class PrivateChatRoom : IPrivateChatRoom
{
    private readonly ChatCooldown _cooldown = new ChatCooldown();
    private readonly IPrivateMessageNotifier _notifier;

    public PrivateChatRoom(IPrivateMessageNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(notifier);

        _notifier = notifier;
    }

    public bool TryClaimTurn(int accountId, DateTime now)
    {
        return _cooldown.TryClaimTurn(accountId, now);
    }

    public void Deliver(PrivateMessageDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        _notifier.NotifyPrivateMessage(delivery);
    }
}
