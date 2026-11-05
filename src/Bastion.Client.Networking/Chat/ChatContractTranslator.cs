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
            log.ConnectedPlayers,
            new ChatMute(log.IsMuted, log.MutedUntil));
    }

    public static ChatMessage ToChatMessage(ChatMessageNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var author = new ChatAuthor(notification.SenderNickname ?? string.Empty, false);
        return new ChatMessage(author, notification.Text ?? string.Empty, notification.OccurredAt);
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
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported rejection reason.");
        }
    }

    private static ChatMessage ToChatMessage(LobbyChatMessage message)
    {
        var author = new ChatAuthor(message.SenderNickname ?? string.Empty, message.IsOwn);
        return new ChatMessage(author, message.Text ?? string.Empty, message.SentAt);
    }
}
