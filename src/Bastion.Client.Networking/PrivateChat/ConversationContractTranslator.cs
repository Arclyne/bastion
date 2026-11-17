using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.PrivateChat;

namespace Bastion.Client.Networking.PrivateChat;

public static class ConversationContractTranslator
{
    public static IReadOnlyList<ConversationPreview> ToPreviews(ConversationEntry[]? entries)
    {
        ConversationEntry[] conversationEntries = entries ?? Array.Empty<ConversationEntry>();
        IEnumerable<ConversationPreview> previews = conversationEntries.Select(ToPreview);
        return new List<ConversationPreview>(previews);
    }

    public static ConversationTranscript ToTranscript(PrivateConversationLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        ConversationContact contact = ToContact(log.PartnerNickname, log.PartnerFriendCode, log.IsFriend);
        PrivateChatMessage[] logMessages = log.Messages ?? Array.Empty<PrivateChatMessage>();
        IEnumerable<PrivateMessage> privateMessages = logMessages.Select(ToPrivateMessage);
        var messages = new List<PrivateMessage>(privateMessages);
        var mute = new ChatMute(log.IsMuted, log.MutedUntil);
        return new ConversationTranscript(contact, messages, mute);
    }

    public static PlayerTag ToSender(PrivateMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return FriendContractTranslator.ToPlayerTag(notification.SenderNickname, notification.SenderFriendCode);
    }

    public static PrivateMessage ToPrivateMessage(PrivateMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string text = notification.Text ?? string.Empty;
        return new PrivateMessage(text, notification.OccurredAt, isOwn: false);
    }

    private static ConversationPreview ToPreview(ConversationEntry entry)
    {
        ConversationContact contact = ToContact(entry.PartnerNickname, entry.PartnerFriendCode, entry.IsFriend);
        PrivateChatMessage lastChatMessage = entry.LastMessage ?? new PrivateChatMessage();
        PrivateMessage lastMessage = ToPrivateMessage(lastChatMessage);
        return new ConversationPreview(contact, lastMessage, entry.UnreadCount);
    }

    private static ConversationContact ToContact(string? partnerNickname, string? partnerFriendCode, bool isFriend)
    {
        var partner = FriendContractTranslator.ToPlayerTag(partnerNickname, partnerFriendCode);
        return new ConversationContact(partner, isFriend);
    }

    private static PrivateMessage ToPrivateMessage(PrivateChatMessage message)
    {
        string text = message.Text ?? string.Empty;
        return new PrivateMessage(text, message.SentAt, message.IsOwn);
    }
}
