using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.PrivateChat;
using Xunit;

namespace Bastion.Client.Tests.PrivateChat;

public sealed class TestConversationContractTranslator
{
    private const string PartnerNickname = "marta_muros";
    private const string PartnerFriendCode = "MA3R7T";
    private const string MessageText = "hello";
    private const int UnreadCount = 2;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToPreviews_Entry_KeepsUnreadCount()
    {
        var lastMessage = new PrivateChatMessage
        {
            Text = MessageText,
            SentAt = _sentAt,
        };
        ConversationEntry entry = CreateEntry(lastMessage);
        ConversationEntry[] entries = { entry };

        IReadOnlyList<ConversationPreview> previews = ConversationContractTranslator.ToPreviews(entries);

        ConversationPreview preview = previews[0];
        Assert.Equal(UnreadCount, preview.UnreadCount);
    }

    [Fact]
    public void ToPreviews_NullEntries_ReturnsEmptyList()
    {
        IReadOnlyList<ConversationPreview> previews = ConversationContractTranslator.ToPreviews(entries: null);

        Assert.Empty(previews);
    }

    [Fact]
    public void ToPreviews_EntryWithoutLastMessage_UsesEmptyText()
    {
        ConversationEntry entry = CreateEntry(lastMessage: null);
        ConversationEntry[] entries = { entry };

        IReadOnlyList<ConversationPreview> previews = ConversationContractTranslator.ToPreviews(entries);

        ConversationPreview preview = previews[0];
        Assert.Empty(preview.LastMessage.Text);
    }

    [Fact]
    public void ToTranscript_OwnMessage_KeepsOwnFlag()
    {
        var ownMessage = new PrivateChatMessage
        {
            Text = MessageText,
            SentAt = _sentAt,
            IsOwn = true,
        };
        PrivateChatMessage[] messages = { ownMessage };
        var log = new PrivateConversationLog
        {
            PartnerNickname = PartnerNickname,
            PartnerFriendCode = PartnerFriendCode,
            IsFriend = true,
            Messages = messages,
        };

        ConversationTranscript transcript = ConversationContractTranslator.ToTranscript(log);

        PrivateMessage first = transcript.Messages[0];
        Assert.True(first.IsOwn);
    }

    [Fact]
    public void ToTranscript_NonFriendPartner_IsNotFriend()
    {
        var log = new PrivateConversationLog
        {
            PartnerNickname = PartnerNickname,
            IsFriend = false,
        };

        ConversationTranscript transcript = ConversationContractTranslator.ToTranscript(log);

        Assert.False(transcript.Contact.IsFriend);
    }

    [Fact]
    public void ToSender_Notification_MapsFriendCode()
    {
        PrivateMessageNotification notification = CreateNotification(PartnerNickname, MessageText);

        PlayerTag sender = ConversationContractTranslator.ToSender(notification);

        Assert.Equal(PartnerFriendCode, sender.FriendCode);
    }

    [Theory]
    [InlineData("rival_one")]
    [InlineData("peón_ñandú")]
    public void ToSender_EnglishOrSpanishNickname_KeepsNickname(string nickname)
    {
        PrivateMessageNotification notification = CreateNotification(nickname, MessageText);

        PlayerTag sender = ConversationContractTranslator.ToSender(notification);

        Assert.Equal(nickname, sender.Nickname);
    }

    [Theory]
    [InlineData("See you in the next match.")]
    [InlineData("¿Jugamos otra? ¡Qué peón, Ñandú!")]
    public void ToPrivateMessage_EnglishOrSpanishText_KeepsText(string text)
    {
        PrivateMessageNotification notification = CreateNotification(PartnerNickname, text);

        var message = ConversationContractTranslator.ToPrivateMessage(notification);

        Assert.Equal(text, message.Text);
    }

    private static ConversationEntry CreateEntry(PrivateChatMessage? lastMessage)
    {
        return new ConversationEntry
        {
            PartnerNickname = PartnerNickname,
            PartnerFriendCode = PartnerFriendCode,
            IsFriend = true,
            LastMessage = lastMessage,
            UnreadCount = UnreadCount,
        };
    }

    private static PrivateMessageNotification CreateNotification(string senderNickname, string text)
    {
        return new PrivateMessageNotification
        {
            SenderNickname = senderNickname,
            SenderFriendCode = PartnerFriendCode,
            Text = text,
            OccurredAt = _sentAt,
        };
    }
}
