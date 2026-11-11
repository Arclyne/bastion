using System;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;
using Bastion.Server.Host.Common;
using CoreWCF;

namespace Bastion.Server.Host.Chat;

public static class ChatTranslator
{
    public static ChatMessageNotification ToLobbyChatNotification(PostedChatLine postedLine)
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

    public static MatchChatMessageNotification ToMatchChatNotification(int matchId, PostedChatLine postedLine)
    {
        ArgumentNullException.ThrowIfNull(postedLine);

        return new MatchChatMessageNotification
        {
            OccurredAt = postedLine.Line.SentAt,
            MatchId = matchId,
            MessageId = postedLine.MessageId,
            SenderNickname = postedLine.Line.Sender.Nickname,
            Text = postedLine.Line.Text,
        };
    }

    public static FaultException<ChatChannelForbiddenFault> ToFault(ChatChannelForbiddenException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new ChatChannelForbiddenFault();
        return FaultExceptionFactory.Create(fault, rejection);
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

        LobbyChatMessage[] messages = ContractArray.ConvertAll(history.Entries, ToMessage);
        return new LobbyChatLog
        {
            Messages = messages,
            ConnectedPlayerCount = history.ConnectedPlayerCount,
            IsMuted = history.Mute.IsMuted,
            MutedUntil = history.Mute.EndsAt,
        };
    }

    public static string RequireText(string? text)
    {
        return text ?? throw ToFault(new ChatMessageRejectedException(ChatRejection.InvalidMessage));
    }

    public static FaultException<ChatMessageRejectedFault> ToFault(ChatMessageRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new ChatMessageRejectedFault
        {
            Reason = ToReason(rejection.Rejection),
            MutedUntil = rejection.MutedUntil,
        };
        return FaultExceptionFactory.Create(fault, rejection);
    }

    private static LobbyChatMessage ToMessage(ChatHistoryEntry entry)
    {
        PostedChatLine postedLine = entry.PostedLine;
        ChatLine line = postedLine.Line;
        return new LobbyChatMessage
        {
            MessageId = postedLine.MessageId,
            SenderNickname = line.Sender.Nickname,
            Text = line.Text,
            SentAt = line.SentAt,
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
            case ChatRejection.NotFriends:
                return ChatRejectionReason.NotFriends;
            case ChatRejection.PlayerNotFound:
                return ChatRejectionReason.PlayerNotFound;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported chat rejection.");
        }
    }
}
