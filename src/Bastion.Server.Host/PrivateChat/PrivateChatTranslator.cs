using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.PrivateChat;
using Bastion.Domain.Chat;
using Bastion.Server.Application.PrivateChat;

namespace Bastion.Server.Host.PrivateChat;

public static class PrivateChatTranslator
{
    public static ConversationEntry[] ToConversationEntries(IReadOnlyList<ConversationSummary> summaries)
    {
        ArgumentNullException.ThrowIfNull(summaries);

        return summaries.Select(ToConversationEntry).ToArray();
    }

    public static PrivateConversationLog ToConversationLog(PrivateConversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        return new PrivateConversationLog
        {
            PartnerNickname = conversation.Partner.Player.Nickname,
            PartnerFriendCode = conversation.Partner.Player.FriendCode,
            IsFriend = conversation.Partner.IsFriend,
            Messages = conversation.Messages.Select(ToChatMessage).ToArray(),
            IsMuted = conversation.Mute.IsMuted,
            MutedUntil = conversation.Mute.EndsAt,
        };
    }

    public static ChatMessageReceipt ToReceipt(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new ChatMessageReceipt
        {
            MessageId = message.MessageId,
            SentAt = message.SentAt,
        };
    }

    public static PrivateMessageNotification ToNotification(PrivateMessageDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        return new PrivateMessageNotification
        {
            OccurredAt = delivery.Message.SentAt,
            MessageId = delivery.Message.MessageId,
            SenderNickname = delivery.Author.Nickname,
            SenderFriendCode = delivery.Author.FriendCode,
            Text = delivery.Message.Text,
        };
    }

    private static ConversationEntry ToConversationEntry(ConversationSummary summary)
    {
        return new ConversationEntry
        {
            PartnerNickname = summary.Partner.Player.Nickname,
            PartnerFriendCode = summary.Partner.Player.FriendCode,
            IsFriend = summary.Partner.IsFriend,
            LastMessage = ToChatMessage(summary.LastMessage),
            UnreadCount = summary.UnreadCount,
        };
    }

    private static PrivateChatMessage ToChatMessage(PrivateMessageEntry entry)
    {
        return new PrivateChatMessage
        {
            MessageId = entry.Message.MessageId,
            Text = entry.Message.Text,
            SentAt = DateTime.SpecifyKind(entry.Message.SentAt, DateTimeKind.Utc),
            IsOwn = entry.IsOwn,
        };
    }
}
