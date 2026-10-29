using System;
using CoreWCF;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Host.Chat;

public static class ChatTranslator
{
    public static ChatMessageNotification ToNotification(ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new ChatMessageNotification
        {
            OccurredAt = line.SentAt,
            SenderNickname = line.SenderNickname,
            Text = line.Text,
        };
    }

    public static FaultException<InvalidChatMessageFault> ToFault(InvalidChatMessageException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        return new FaultException<InvalidChatMessageFault>(
            new InvalidChatMessageFault(),
            new FaultReason(rejection.Message));
    }
}
