using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Chat;

public static class ChatContractTranslator
{
    public static LobbyChatTranscript ToTranscript(LobbyChatLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        LobbyChatMessage[] messages = log.Messages ?? Array.Empty<LobbyChatMessage>();
        IEnumerable<ChatMessage> chatMessages = messages.Select(ToChatMessage);
        var messageList = new List<ChatMessage>(chatMessages);
        var mute = new ChatMute(log.IsMuted, log.MutedUntil);
        return new LobbyChatTranscript(messageList, log.ConnectedPlayerCount, mute);
    }

    public static ChatMessage ToChatMessage(ChatMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return ToReceivedMessage(notification.SenderNickname, notification.Text, notification.OccurredAt);
    }

    public static ChatMessage ToChatMessage(MatchChatMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return ToReceivedMessage(notification.SenderNickname, notification.Text, notification.OccurredAt);
    }

    public static ChatRejection ToRejection(ChatRejectionReason reason)
    {
        switch (reason)
        {
            case ChatRejectionReason.InvalidMessage:
                return ChatRejection.InvalidMessage;
            case ChatRejectionReason.Muted:
                return ChatRejection.Muted;
            case ChatRejectionReason.BannedTerm:
                return ChatRejection.BannedTerm;
            case ChatRejectionReason.TooFast:
                return ChatRejection.TooFast;
            case ChatRejectionReason.NotFriends:
                return ChatRejection.NotFriends;
            case ChatRejectionReason.PlayerNotFound:
                return ChatRejection.PlayerNotFound;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported rejection reason.");
        }
    }

    private static ChatMessage ToChatMessage(LobbyChatMessage message)
    {
        var author = new ChatAuthor(message.SenderNickname ?? string.Empty, message.IsOwn);
        string text = message.Text ?? string.Empty;
        return new ChatMessage(author, text, message.SentAt);
    }

    private static ChatMessage ToReceivedMessage(string? senderNickname, string? text, DateTime sentAt)
    {
        var author = new ChatAuthor(senderNickname ?? string.Empty, false);
        string messageText = text ?? string.Empty;
        return new ChatMessage(author, messageText, sentAt);
    }
}
