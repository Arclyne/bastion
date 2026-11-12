using System;
using System.Collections.Generic;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.PrivateChat;
using Bastion.Domain.Chat;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Host.Common;

namespace Bastion.Server.Host.PrivateChat;

public static class PrivateChatTranslator
{
    public static ConversationEntry[] ToConversationEntries(IReadOnlyList<ConversationSummary> summaries)
    {
        ArgumentNullException.ThrowIfNull(summaries);

        ConversationEntry[] entries = ContractArray.ConvertAll(summaries, ToConversationEntry);
        return entries;
    }

    public static PrivateConversationLog ToConversationLog(PrivateConversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        PrivateChatMessage[] messages = ContractArray.ConvertAll(conversation.Messages, ToPrivateChatMessage);
        return new PrivateConversationLog
        {
            PartnerNickname = conversation.Partner.Player.Nickname,
            PartnerFriendCode = conversation.Partner.Player.FriendCode,
            IsFriend = conversation.Partner.IsFriend,
            Messages = messages,
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

    public static PrivateMessageNotification ToPrivateMessageNotification(PrivateMessageDelivery delivery)
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
        PrivateChatMessage lastMessage = ToPrivateChatMessage(summary.LastMessage);
        return new ConversationEntry
        {
            PartnerNickname = summary.Partner.Player.Nickname,
            PartnerFriendCode = summary.Partner.Player.FriendCode,
            IsFriend = summary.Partner.IsFriend,
            LastMessage = lastMessage,
            UnreadCount = summary.UnreadCount,
        };
    }

    private static PrivateChatMessage ToPrivateChatMessage(PrivateMessageEntry entry)
    {
        var sentAt = DateTime.SpecifyKind(entry.Message.SentAt, DateTimeKind.Utc);
        return new PrivateChatMessage
        {
            MessageId = entry.Message.MessageId,
            Text = entry.Message.Text,
            SentAt = sentAt,
            IsOwn = entry.IsOwn,
        };
    }
}
