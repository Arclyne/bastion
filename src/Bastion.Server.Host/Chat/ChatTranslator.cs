using System;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;
using CoreWCF;

namespace Bastion.Server.Host.Chat;

public static class ChatTranslator
{
    public static LobbyChatMessageNotification ToLobbyChatNotification(ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new LobbyChatMessageNotification
        {
            OccurredAt = line.SentAt,
            SenderNickname = line.SenderNickname,
            Text = line.Text,
        };
    }

    public static MatchChatMessageNotification ToMatchChatNotification(int matchId, ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new MatchChatMessageNotification
        {
            OccurredAt = line.SentAt,
            MatchId = matchId,
            SenderNickname = line.SenderNickname,
            Text = line.Text,
        };
    }

    public static FaultException<ChatChannelForbiddenFault> ToFault(ChatChannelForbiddenException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        return new FaultException<ChatChannelForbiddenFault>(
            new ChatChannelForbiddenFault(),
            new FaultReason(rejection.Message));
    }

    public static FaultException<InvalidChatMessageFault> ToFault(InvalidChatMessageException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        return new FaultException<InvalidChatMessageFault>(
            new InvalidChatMessageFault(),
            new FaultReason(rejection.Message));
    }
}
