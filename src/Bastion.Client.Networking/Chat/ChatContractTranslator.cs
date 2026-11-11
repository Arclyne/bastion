using System;
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
        return new LobbyChatTranscript(
            messages.Select(ToChatMessage).ToList(),
            log.ConnectedPlayerCount,
            new ChatMute(log.IsMuted, log.MutedUntil));
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
        return new ChatMessage(author, message.Text ?? string.Empty, message.SentAt);
    }

    private static ChatMessage ToReceivedMessage(string? senderNickname, string? text, DateTime sentAt)
    {
        var author = new ChatAuthor(senderNickname ?? string.Empty, false);
        return new ChatMessage(author, text ?? string.Empty, sentAt);
    }
}
