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

        var partner = FriendContractTranslator.ToPlayerTag(log.PartnerNickname, log.PartnerFriendCode);
        var contact = new ConversationContact(partner, log.IsFriend);
        PrivateChatMessage[] messages = log.Messages ?? Array.Empty<PrivateChatMessage>();
        IEnumerable<PrivateMessage> privateMessages = messages.Select(ToPrivateMessage);
        var messageList = new List<PrivateMessage>(privateMessages);
        var mute = new ChatMute(log.IsMuted, log.MutedUntil);
        return new ConversationTranscript(contact, messageList, mute);
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
        return new PrivateMessage(text, notification.OccurredAt, false);
    }

    private static ConversationPreview ToPreview(ConversationEntry entry)
    {
        var partner = FriendContractTranslator.ToPlayerTag(entry.PartnerNickname, entry.PartnerFriendCode);
        var contact = new ConversationContact(partner, entry.IsFriend);
        PrivateMessage lastMessage = entry.LastMessage is null
            ? new PrivateMessage(string.Empty, DateTime.MinValue, false)
            : ToPrivateMessage(entry.LastMessage);
        return new ConversationPreview(contact, lastMessage, entry.UnreadCount);
    }

    private static PrivateMessage ToPrivateMessage(PrivateChatMessage message)
    {
        string text = message.Text ?? string.Empty;
        return new PrivateMessage(text, message.SentAt, message.IsOwn);
    }
}
