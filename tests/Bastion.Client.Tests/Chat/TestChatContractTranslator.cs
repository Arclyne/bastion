using System;
using Bastion.Client.Networking.Chat;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Notifications;
using Xunit;

namespace Bastion.Client.Tests.Chat;

public sealed class TestChatContractTranslator
{
    private const string SenderNickname = "luis_quo";
    private const string Text = "hello";
    private const int ConnectedPlayerCount = 4;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToTranscript_OwnMessage_KeepsOwnFlag()
    {
        LobbyChatLog log = CreateOwnMessageLog();

        LobbyChatTranscript transcript = ChatContractTranslator.ToTranscript(log);

        ChatMessage first = transcript.Messages[0];
        Assert.True(first.Author.IsOwn);
    }

    [Fact]
    public void ToTranscript_NullMessages_ReturnsEmptyList()
    {
        var log = new LobbyChatLog { ConnectedPlayerCount = ConnectedPlayerCount };

        LobbyChatTranscript transcript = ChatContractTranslator.ToTranscript(log);

        Assert.Empty(transcript.Messages);
    }

    [Fact]
    public void ToTranscript_MutedLog_KeepsMuteEnd()
    {
        var log = new LobbyChatLog
        {
            IsMuted = true,
            MutedUntil = _sentAt,
        };
        var expectedMute = new ChatMute(IsMuted: true, EndsAt: _sentAt);

        LobbyChatTranscript transcript = ChatContractTranslator.ToTranscript(log);

        Assert.Equal(expectedMute, transcript.Mute);
    }

    [Fact]
    public void ToChatMessage_Notification_IsNotOwn()
    {
        ChatMessageNotification notification = CreateNotification(SenderNickname, Text);

        var message = ChatContractTranslator.ToChatMessage(notification);

        Assert.False(message.Author.IsOwn);
    }

    [Theory]
    [InlineData("Good game, well played!")]
    [InlineData("¿Otra partida? ¡Qué peón, Ñandú!")]
    public void ToChatMessage_EnglishOrSpanishText_KeepsText(string text)
    {
        ChatMessageNotification notification = CreateNotification(SenderNickname, text);

        var message = ChatContractTranslator.ToChatMessage(notification);

        Assert.Equal(text, message.Text);
    }

    [Theory]
    [InlineData("rival_one")]
    [InlineData("peón_ñandú")]
    public void ToChatMessage_EnglishOrSpanishNickname_KeepsNickname(string nickname)
    {
        ChatMessageNotification notification = CreateNotification(nickname, Text);

        var message = ChatContractTranslator.ToChatMessage(notification);

        Assert.Equal(nickname, message.Author.Nickname);
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

    private static LobbyChatLog CreateOwnMessageLog()
    {
        var message = new LobbyChatMessage
        {
            SenderNickname = SenderNickname,
            Text = Text,
            SentAt = _sentAt,
            IsOwn = true,
        };
        LobbyChatMessage[] messages = { message };
        return new LobbyChatLog
        {
            Messages = messages,
            ConnectedPlayerCount = ConnectedPlayerCount,
        };
    }

    private static ChatMessageNotification CreateNotification(string senderNickname, string text)
    {
        return new ChatMessageNotification
        {
            SenderNickname = senderNickname,
            Text = text,
            OccurredAt = _sentAt,
        };
    }
}
