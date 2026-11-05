using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.PrivateChat;
using Xunit;

namespace Bastion.Client.Tests.PrivateChat;

public class TestConversationContractTranslator
{
    private const string PartnerNickname = "marta_muros";
    private const string PartnerFriendCode = "MA3R7T";
    private const int UnreadCount = 2;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToPreviews_Entry_KeepsUnreadCount()
    {
        ConversationEntry[] entries = { CreateEntry(new PrivateChatMessage { Text = "hola", SentAt = _sentAt }) };

        IReadOnlyList<ConversationPreview> previews = ConversationContractTranslator.ToPreviews(entries);

        Assert.Equal(UnreadCount, previews[0].UnreadCount);
    }

    [Fact]
    public void ToPreviews_NullEntries_ReturnsEmptyList()
    {
        IReadOnlyList<ConversationPreview> previews = ConversationContractTranslator.ToPreviews(null);

        Assert.Empty(previews);
    }

    [Fact]
    public void ToPreviews_EntryWithoutLastMessage_UsesEmptyText()
    {
        ConversationEntry[] entries = { CreateEntry(null) };

        IReadOnlyList<ConversationPreview> previews = ConversationContractTranslator.ToPreviews(entries);

        Assert.Empty(previews[0].LastMessage.Text);
    }

    [Fact]
    public void ToTranscript_OwnMessage_KeepsOwnFlag()
    {
        var log = new PrivateConversationLog
        {
            PartnerNickname = PartnerNickname,
            PartnerFriendCode = PartnerFriendCode,
            IsFriend = true,
            Messages = new[] { new PrivateChatMessage { Text = "hola", SentAt = _sentAt, IsOwn = true } },
        };

        ConversationTranscript transcript = ConversationContractTranslator.ToTranscript(log);

        Assert.True(transcript.Messages[0].IsOwn);
    }

    [Fact]
    public void ToTranscript_FormerFriend_IsNotFriend()
    {
        var log = new PrivateConversationLog { PartnerNickname = PartnerNickname, IsFriend = false };

        ConversationTranscript transcript = ConversationContractTranslator.ToTranscript(log);

        Assert.False(transcript.Contact.IsFriend);
    }

    [Fact]
    public void ToSender_Notification_MapsFriendCode()
    {
        var notification = new PrivateMessageNotification
        {
            SenderNickname = PartnerNickname,
            SenderFriendCode = PartnerFriendCode,
            Text = "hola",
            OccurredAt = _sentAt,
        };

        PlayerTag sender = ConversationContractTranslator.ToSender(notification);

        Assert.Equal(PartnerFriendCode, sender.FriendCode);
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
}
