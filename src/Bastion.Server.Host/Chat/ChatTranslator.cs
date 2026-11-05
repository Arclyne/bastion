using System;
using System.Linq;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;
using CoreWCF;

namespace Bastion.Server.Host.Chat;

public static class ChatTranslator
{
    public static ChatMessageNotification ToNotification(PostedChatLine postedLine)
    {
        ArgumentNullException.ThrowIfNull(postedLine);

        return new ChatMessageNotification
        {
            OccurredAt = postedLine.Line.SentAt,
            MessageId = postedLine.MessageId,
            SenderNickname = postedLine.Line.Sender.Nickname,
            Text = postedLine.Line.Text,
        };
    }

    public static ChatMessageReceipt ToReceipt(PostedChatLine postedLine)
    {
        ArgumentNullException.ThrowIfNull(postedLine);

        return new ChatMessageReceipt
        {
            MessageId = postedLine.MessageId,
            SentAt = postedLine.Line.SentAt,
        };
    }

    public static LobbyChatLog ToChatLog(LobbyChatHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        return new LobbyChatLog
        {
            Messages = history.Entries.Select(ToMessage).ToArray(),
            ConnectedPlayers = history.ConnectedPlayers,
            IsMuted = history.Mute.IsMuted,
            MutedUntil = history.Mute.EndsAt,
        };
    }

    public static FaultException<ChatMessageRejectedFault> ToFault(ChatMessageRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new ChatMessageRejectedFault
        {
            Reason = ToReason(rejection.Rejection),
            MutedUntil = rejection.MutedUntil,
        };
        return new FaultException<ChatMessageRejectedFault>(fault, new FaultReason(rejection.Message));
    }

    private static LobbyChatMessage ToMessage(ChatHistoryEntry entry)
    {
        return new LobbyChatMessage
        {
            MessageId = entry.Line.MessageId,
            SenderNickname = entry.Line.Line.Sender.Nickname,
            Text = entry.Line.Line.Text,
            SentAt = entry.Line.Line.SentAt,
            IsOwn = entry.IsOwn,
        };
    }

    private static ChatRejectionReason ToReason(ChatRejection rejection)
    {
        switch (rejection)
        {
            case ChatRejection.InvalidMessage:
                return ChatRejectionReason.InvalidMessage;
            case ChatRejection.Muted:
                return ChatRejectionReason.Muted;
            case ChatRejection.BannedTerm:
                return ChatRejectionReason.BannedTerm;
            case ChatRejection.TooFast:
                return ChatRejectionReason.TooFast;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported chat rejection.");
        }
    }
}
