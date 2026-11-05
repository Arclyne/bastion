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
        return (entries ?? Array.Empty<ConversationEntry>()).Select(ToPreview).ToList();
    }

    public static ConversationTranscript ToTranscript(PrivateConversationLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var contact = new ConversationContact(
            new PlayerTag(log.PartnerNickname ?? string.Empty, log.PartnerFriendCode ?? string.Empty),
            log.IsFriend);
        PrivateChatMessage[] messages = log.Messages ?? Array.Empty<PrivateChatMessage>();
        return new ConversationTranscript(
            contact,
            messages.Select(ToPrivateMessage).ToList(),
            new ChatMute(log.IsMuted, log.MutedUntil));
    }

    public static PlayerTag ToSender(PrivateMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return new PlayerTag(
            notification.SenderNickname ?? string.Empty,
            notification.SenderFriendCode ?? string.Empty);
    }

    public static PrivateMessage ToPrivateMessage(PrivateMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return new PrivateMessage(notification.Text ?? string.Empty, notification.OccurredAt, false);
    }

    private static ConversationPreview ToPreview(ConversationEntry entry)
    {
        var contact = new ConversationContact(
            new PlayerTag(entry.PartnerNickname ?? string.Empty, entry.PartnerFriendCode ?? string.Empty),
            entry.IsFriend);
        PrivateMessage lastMessage = entry.LastMessage is null
            ? new PrivateMessage(string.Empty, DateTime.MinValue, false)
            : ToPrivateMessage(entry.LastMessage);
        return new ConversationPreview(contact, lastMessage, entry.UnreadCount);
    }

    private static PrivateMessage ToPrivateMessage(PrivateChatMessage message)
    {
        return new PrivateMessage(message.Text ?? string.Empty, message.SentAt, message.IsOwn);
    }
}
