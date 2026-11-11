using System;
using Bastion.Client.Networking.Chat;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Xunit;

namespace Bastion.Client.Tests.Chat;

public class TestChatContractTranslator
{
    private const string SenderNickname = "luis_quo";
    private const string Text = "hola";
    private const int ConnectedPlayerCount = 4;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToTranscript_OwnMessage_KeepsOwnFlag()
    {
        var message = new LobbyChatMessage { SenderNickname = SenderNickname, Text = Text, IsOwn = true };
        LobbyChatLog log = CreateLog(message);

        LobbyChatTranscript transcript = ChatContractTranslator.ToTranscript(log);

        Assert.True(transcript.Messages[0].Author.IsOwn);
    }

    [Fact]
    public void ToTranscript_NullMessages_ReturnsEmptyList()
    {
        var log = new LobbyChatLog { ConnectedPlayerCount = ConnectedPlayerCount };

        LobbyChatTranscript transcript = ChatContractTranslator.ToTranscript(log);

        Assert.Empty(transcript.Messages);
    }

    [Fact]
    public void ToTranscript_MutedLog_MapsMuteEnd()
    {
        var log = new LobbyChatLog { IsMuted = true, MutedUntil = _sentAt };

        LobbyChatTranscript transcript = ChatContractTranslator.ToTranscript(log);

        Assert.Equal(new ChatMute(true, _sentAt), transcript.Mute);
    }

    [Fact]
    public void ToChatMessage_Notification_IsNotOwn()
    {
        var notification = new ChatMessageNotification
        {
            SenderNickname = SenderNickname,
            Text = Text,
            OccurredAt = _sentAt,
        };

        var message = ChatContractTranslator.ToChatMessage(notification);

        Assert.False(message.Author.IsOwn);
    }

    [Theory]
    [InlineData(ChatRejectionReason.InvalidMessage, ChatRejection.InvalidMessage)]
    [InlineData(ChatRejectionReason.Muted, ChatRejection.Muted)]
    [InlineData(ChatRejectionReason.BannedTerm, ChatRejection.BannedTerm)]
    [InlineData(ChatRejectionReason.TooFast, ChatRejection.TooFast)]
    public void ToRejection_KnownReason_MapsRejection(ChatRejectionReason reason, ChatRejection expected)
    {
        ChatRejection rejection = ChatContractTranslator.ToRejection(reason);

        Assert.Equal(expected, rejection);
    }

    private static LobbyChatLog CreateLog(LobbyChatMessage message)
    {
        message.SentAt = _sentAt;
        return new LobbyChatLog
        {
            Messages = new[] { message },
            ConnectedPlayerCount = ConnectedPlayerCount,
        };
    }
}
