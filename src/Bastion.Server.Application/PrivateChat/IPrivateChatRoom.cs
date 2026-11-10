using System;

namespace Bastion.Server.Application.PrivateChat;

public interface IPrivateChatRoom
{
    bool TryStartCooldown(int accountId, DateTime now);

    void Deliver(PrivateMessageDelivery delivery);
}
