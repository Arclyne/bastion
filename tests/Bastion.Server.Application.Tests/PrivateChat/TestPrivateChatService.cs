using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.PrivateChat;

public class TestPrivateChatService
{
    private const int AccountId = 3;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Token = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const int FriendAccountId = 4;
    private const string FriendNickname = "marta_muros";
    private const string FriendFriendCode = "MA3R7T";
    private const int StrangerAccountId = 15;
    private const string StrangerNickname = "clara_path";
    private const string StrangerFriendCode = "CP8T9H";
    private const string UnknownFriendCode = "ZZ9ZZ9";
    private const string BannedTerm = "tonto";

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakePrivateMessageNotifier _notifier = new FakePrivateMessageNotifier();
    private readonly PrivateChatService _privateChatService;

    public TestPrivateChatService()
    {
        var channel = new PrivateChatChannel(new PrivateChatRoom(_notifier), new ChatModerator(_unitOfWork));
        _privateChatService = new PrivateChatService(_sessionVerifier, _unitOfWork, channel);
        _sessionVerifier.Add(Token, new PlayerIdentity(AccountId, Nickname, FriendCode));
        var friend = new PlayerIdentity(FriendAccountId, FriendNickname, FriendFriendCode);
        _unitOfWork.AccountStore.StorePlayerIdentity(new PlayerIdentity(AccountId, Nickname, FriendCode));
        _unitOfWork.AccountStore.StorePlayerIdentity(friend);
        var stranger = new PlayerIdentity(StrangerAccountId, StrangerNickname, StrangerFriendCode);
        _unitOfWork.AccountStore.StorePlayerIdentity(stranger);
        _unitOfWork.FriendshipStore.Store(AccountId, new FriendSummary(friend, null));
        _unitOfWork.BannedWordStore.ChatTerms.Add(BannedTerm);
    }

    [Fact]
    public void SendPrivateMessage_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() =>
            _privateChatService.SendPrivateMessage(UnknownToken, ToFriend("hola"), _now));
    }

    [Fact]
    public void SendPrivateMessage_Friend_StoresTrimmedText()
    {
        _privateChatService.SendPrivateMessage(Token, ToFriend("  hola  "), _now);

        Assert.Equal("hola", _unitOfWork.MessageStore.Messages[0].Text);
    }

    [Fact]
    public void SendPrivateMessage_Friend_SavesChanges()
    {
        _privateChatService.SendPrivateMessage(Token, ToFriend("hola"), _now);

        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public void SendPrivateMessage_Friend_DeliversWithAuthorFriendCode()
    {
        _privateChatService.SendPrivateMessage(Token, ToFriend("hola"), _now);

        Assert.Equal(FriendCode, _notifier.Deliveries[0].Author.FriendCode);
    }

    [Fact]
    public void SendPrivateMessage_Friend_AddressesTheFriend()
    {
        Message message = _privateChatService.SendPrivateMessage(Token, ToFriend("hola"), _now);

        Assert.Equal(FriendAccountId, message.RecipientId);
    }

    [Fact]
    public void SendPrivateMessage_FormattedFriendCode_FindsTheFriend()
    {
        _privateChatService.SendPrivateMessage(Token, new PrivateMessageDraft("ma-3r7t", "hola"), _now);

        Assert.Single(_unitOfWork.MessageStore.Messages);
    }

    [Fact]
    public void SendPrivateMessage_NotAFriend_RejectsNotFriends()
    {
        ChatMessageRejectedException? rejection = RecordSendRejection(
            new PrivateMessageDraft(StrangerFriendCode, "hola"),
            _now);

        Assert.Equal(ChatRejection.NotFriends, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_UnknownCode_RejectsPlayerNotFound()
    {
        ChatMessageRejectedException? rejection = RecordSendRejection(
            new PrivateMessageDraft(UnknownFriendCode, "hola"),
            _now);

        Assert.Equal(ChatRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_OwnCode_RejectsPlayerNotFound()
    {
        var ownDraft = new PrivateMessageDraft(FriendCode, "hola");

        ChatMessageRejectedException? rejection = RecordSendRejection(ownDraft, _now);

        Assert.Equal(ChatRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_BannedTerm_RejectsBannedTerm()
    {
        ChatMessageRejectedException? rejection = RecordSendRejection(ToFriend("eres un tonto"), _now);

        Assert.Equal(ChatRejection.BannedTerm, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_ChatSanction_RejectsMuted()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(1)));

        ChatMessageRejectedException? rejection = RecordSendRejection(ToFriend("hola"), _now);

        Assert.Equal(ChatRejection.Muted, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_SecondMessageWithinCooldown_RejectsTooFast()
    {
        _privateChatService.SendPrivateMessage(Token, ToFriend("hola"), _now);

        ChatMessageRejectedException? rejection = RecordSendRejection(ToFriend("otra vez"), _now.AddSeconds(1));

        Assert.Equal(ChatRejection.TooFast, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_Rejected_DoesNotStore()
    {
        Record.Exception(() => _privateChatService.SendPrivateMessage(Token, ToFriend("tonto"), _now));

        Assert.Empty(_unitOfWork.MessageStore.Messages);
    }

    [Fact]
    public void OpenConversation_Friend_ReturnsMessagesInOrder()
    {
        StoreMessage(FriendAccountId, AccountId, "primero");
        StoreMessage(AccountId, FriendAccountId, "segundo");

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.Equal(new[] { "primero", "segundo" }, conversation.Messages.Select(entry => entry.Message.Text));
    }

    [Fact]
    public void OpenConversation_OwnMessage_MarksItAsOwn()
    {
        StoreMessage(AccountId, FriendAccountId, "hola");

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.True(conversation.Messages[0].IsOwn);
    }

    [Fact]
    public void OpenConversation_UnreadMessage_MarksItRead()
    {
        Message received = StoreMessage(FriendAccountId, AccountId, "hola");

        _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.Equal(_now, received.ReadAt);
    }

    [Fact]
    public void OpenConversation_OwnUnreadMessage_StaysUnread()
    {
        Message sent = StoreMessage(AccountId, FriendAccountId, "hola");

        _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.Null(sent.ReadAt);
    }

    [Fact]
    public void OpenConversation_Friend_CanWrite()
    {
        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.True(conversation.Partner.IsFriend);
    }

    [Fact]
    public void OpenConversation_FormerFriendWithHistory_ReturnsReadOnly()
    {
        StoreMessage(StrangerAccountId, AccountId, "¿me pasas la sala?");

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, StrangerFriendCode, _now);

        Assert.False(conversation.Partner.IsFriend);
    }

    [Fact]
    public void OpenConversation_StrangerWithoutHistory_RejectsNotFriends()
    {
        var rejection = Record.Exception(() =>
            _privateChatService.OpenConversation(Token, StrangerFriendCode, _now)) as ChatMessageRejectedException;

        Assert.Equal(ChatRejection.NotFriends, rejection?.Rejection);
    }

    [Fact]
    public void OpenConversation_ChatSanction_ReturnsMute()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Chat, null));

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.True(conversation.Mute.IsMuted);
    }

    [Fact]
    public void ListConversations_TwoPartners_OrdersByLatestMessage()
    {
        StoreMessage(StrangerAccountId, AccountId, "viejo", _now.AddDays(-2));
        StoreMessage(FriendAccountId, AccountId, "nuevo", _now.AddMinutes(-1));

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        Assert.Equal(
            new[] { FriendAccountId, StrangerAccountId },
            conversations.Select(summary => summary.Partner.Player.AccountId));
    }

    [Fact]
    public void ListConversations_SeveralMessages_ShowsTheLatest()
    {
        StoreMessage(FriendAccountId, AccountId, "primero", _now.AddMinutes(-2));
        StoreMessage(AccountId, FriendAccountId, "último", _now.AddMinutes(-1));

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        Assert.Equal("último", conversations[0].LastMessage.Message.Text);
    }

    [Fact]
    public void ListConversations_UnreadMessages_CountsOnlyReceived()
    {
        StoreMessage(FriendAccountId, AccountId, "uno", _now.AddMinutes(-3));
        StoreMessage(FriendAccountId, AccountId, "dos", _now.AddMinutes(-2));
        StoreMessage(AccountId, FriendAccountId, "mío", _now.AddMinutes(-1));

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        Assert.Equal(2, conversations[0].UnreadCount);
    }

    [Fact]
    public void ListConversations_FormerFriend_IsNotFriend()
    {
        StoreMessage(StrangerAccountId, AccountId, "hola");

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        Assert.False(conversations[0].Partner.IsFriend);
    }

    [Fact]
    public void MarkConversationRead_UnreadMessage_MarksItRead()
    {
        Message received = StoreMessage(FriendAccountId, AccountId, "hola");

        _privateChatService.MarkConversationRead(Token, FriendFriendCode, _now);

        Assert.Equal(_now, received.ReadAt);
    }

    private static PrivateMessageDraft ToFriend(string text)
    {
        return new PrivateMessageDraft(FriendFriendCode, text);
    }

    private ChatMessageRejectedException? RecordSendRejection(PrivateMessageDraft draft, DateTime now)
    {
        return Record.Exception(() => _privateChatService.SendPrivateMessage(Token, draft, now))
            as ChatMessageRejectedException;
    }

    private Message StoreMessage(int authorId, int recipientId, string text)
    {
        return StoreMessage(authorId, recipientId, text, _now.AddMinutes(-1));
    }

    private Message StoreMessage(int authorId, int recipientId, string text, DateTime sentAt)
    {
        var message = Message.CreatePrivate(new MessageParticipants(authorId, recipientId), text, sentAt);
        _unitOfWork.MessageStore.Add(message);
        return message;
    }
}
