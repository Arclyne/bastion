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

public sealed class TestPrivateChatService
{
    private const int AccountId = 3;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Token = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const int FriendAccountId = 4;
    private const string FriendNickname = "marta_muros";
    private const string FriendFriendCode = "MA3R7T";
    private const string HyphenatedFriendFriendCode = "ma-3r7t";
    private const int StrangerAccountId = 15;
    private const string StrangerNickname = "clara_path";
    private const string StrangerFriendCode = "CP8T9H";
    private const string UnknownFriendCode = "ZZ9ZZ9";
    private const string BannedTerm = "fool";
    private const string CleanText = "hello";
    private const string PaddedText = "  hello  ";
    private const string BannedText = "you are a fool";
    private const string RepeatedText = "again";
    private const string FirstText = "first";
    private const string SecondText = "second";
    private const string StrangerText = "can you share the room code?";
    private const string OldText = "old";
    private const string NewText = "new";
    private const string LastText = "last";
    private const string FirstUnreadText = "one";
    private const string SecondUnreadText = "two";
    private const string OwnText = "mine";
    private const int OneSave = 1;
    private const int TwoUnread = 2;
    private const int SecondsWithinCooldown = 1;
    private const int SanctionDays = 1;
    private const int OlderMessageDays = 2;
    private const int OneMinute = 1;
    private const int TwoMinutes = 2;
    private const int ThreeMinutes = 3;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _oneMinuteAgo = _now.AddMinutes(-OneMinute);
    private static readonly DateTime _twoMinutesAgo = _now.AddMinutes(-TwoMinutes);
    private static readonly PlayerIdentity _caller = new PlayerIdentity(AccountId, Nickname, FriendCode);
    private static readonly PlayerIdentity _friend = new PlayerIdentity(FriendAccountId, FriendNickname, FriendFriendCode);
    private static readonly MessageRoute _friendToCaller = new MessageRoute(FriendAccountId, AccountId);
    private static readonly MessageRoute _callerToFriend = new MessageRoute(AccountId, FriendAccountId);
    private static readonly MessageRoute _strangerToCaller = new MessageRoute(StrangerAccountId, AccountId);
    private static readonly PlayerIdentity _stranger = new PlayerIdentity(
        StrangerAccountId,
        StrangerNickname,
        StrangerFriendCode);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakePrivateMessageNotifier _notifier = new FakePrivateMessageNotifier();
    private readonly PrivateChatService _privateChatService;

    public TestPrivateChatService()
    {
        var chatRoom = new PrivateChatRoom(_notifier);
        var moderator = new ChatModerator(_unitOfWork);
        var channel = new PrivateChatChannel(chatRoom, moderator);
        _privateChatService = new PrivateChatService(_sessionVerifier, _unitOfWork, channel);
    }

    [Fact]
    public void SendPrivateMessage_InvalidSession_ThrowsInvalidSessionException()
    {
        PrivateMessageDraft draft = CreateDraftToFriend(CleanText);

        Action send = () => _privateChatService.SendPrivateMessage(UnknownToken, draft, _now);

        Assert.Throws<InvalidSessionException>(send);
    }

    [Fact]
    public void SendPrivateMessage_Friend_StoresTrimmedText()
    {
        AddPlayers();
        PrivateMessageDraft draft = CreateDraftToFriend(PaddedText);

        _privateChatService.SendPrivateMessage(Token, draft, _now);

        Message storedMessage = _unitOfWork.MessageRepository.Messages[0];
        Assert.Equal(CleanText, storedMessage.Text);
    }

    [Theory]
    [InlineData("See you in the next match.")]
    [InlineData("¿Jugamos otra? ¡Qué peón, Ñandú!")]
    public void SendPrivateMessage_EnglishOrSpanishText_StoresSameText(string text)
    {
        AddPlayers();
        PrivateMessageDraft draft = CreateDraftToFriend(text);

        _privateChatService.SendPrivateMessage(Token, draft, _now);

        Message storedMessage = _unitOfWork.MessageRepository.Messages[0];
        Assert.Equal(text, storedMessage.Text);
    }

    [Fact]
    public void SendPrivateMessage_Friend_SavesChangesOnce()
    {
        AddPlayers();
        PrivateMessageDraft draft = CreateDraftToFriend(CleanText);

        _privateChatService.SendPrivateMessage(Token, draft, _now);

        Assert.Equal(OneSave, _unitOfWork.SaveCount);
    }

    [Fact]
    public void SendPrivateMessage_Friend_DeliversWithAuthorFriendCode()
    {
        AddPlayers();
        PrivateMessageDraft draft = CreateDraftToFriend(CleanText);

        _privateChatService.SendPrivateMessage(Token, draft, _now);

        PrivateMessageDelivery delivery = _notifier.Deliveries[0];
        Assert.Equal(FriendCode, delivery.Author.FriendCode);
    }

    [Fact]
    public void SendPrivateMessage_Friend_AddressesTheFriend()
    {
        AddPlayers();
        PrivateMessageDraft draft = CreateDraftToFriend(CleanText);

        Message message = _privateChatService.SendPrivateMessage(Token, draft, _now);

        Assert.Equal(FriendAccountId, message.RecipientId);
    }

    [Fact]
    public void SendPrivateMessage_HyphenatedFriendCode_StoresMessage()
    {
        AddPlayers();
        var draft = new PrivateMessageDraft(HyphenatedFriendFriendCode, CleanText);

        _privateChatService.SendPrivateMessage(Token, draft, _now);

        Assert.Single(_unitOfWork.MessageRepository.Messages);
    }

    [Fact]
    public void SendPrivateMessage_NotAFriend_RejectsNotFriends()
    {
        AddPlayers();
        var draft = new PrivateMessageDraft(StrangerFriendCode, CleanText);
        Action send = () => _privateChatService.SendPrivateMessage(Token, draft, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.NotFriends, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_UnknownCode_RejectsPlayerNotFound()
    {
        AddPlayers();
        var draft = new PrivateMessageDraft(UnknownFriendCode, CleanText);
        Action send = () => _privateChatService.SendPrivateMessage(Token, draft, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_OwnCode_RejectsPlayerNotFound()
    {
        AddPlayers();
        var ownDraft = new PrivateMessageDraft(FriendCode, CleanText);
        Action send = () => _privateChatService.SendPrivateMessage(Token, ownDraft, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_BannedTerm_RejectsBannedTerm()
    {
        AddPlayers();
        _unitOfWork.BannedWordRepository.ChatTerms.Add(BannedTerm);
        PrivateMessageDraft draft = CreateDraftToFriend(BannedText);
        Action send = () => _privateChatService.SendPrivateMessage(Token, draft, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.BannedTerm, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_ChatSanction_RejectsMuted()
    {
        AddPlayers();
        DateTime muteEnd = _now.AddDays(SanctionDays);
        AddChatSanction(muteEnd);
        PrivateMessageDraft draft = CreateDraftToFriend(CleanText);
        Action send = () => _privateChatService.SendPrivateMessage(Token, draft, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.Muted, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_SecondMessageWithinCooldown_RejectsTooFast()
    {
        AddPlayers();
        PrivateMessageDraft firstDraft = CreateDraftToFriend(CleanText);
        _privateChatService.SendPrivateMessage(Token, firstDraft, _now);
        PrivateMessageDraft repeatedDraft = CreateDraftToFriend(RepeatedText);
        DateTime withinCooldown = _now.AddSeconds(SecondsWithinCooldown);
        Action sendAgain = () => _privateChatService.SendPrivateMessage(Token, repeatedDraft, withinCooldown);

        Exception? exception = Record.Exception(sendAgain);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.TooFast, rejection?.Rejection);
    }

    [Fact]
    public void SendPrivateMessage_Rejected_DoesNotStore()
    {
        AddPlayers();
        _unitOfWork.BannedWordRepository.ChatTerms.Add(BannedTerm);
        PrivateMessageDraft draft = CreateDraftToFriend(BannedTerm);
        Action send = () => _privateChatService.SendPrivateMessage(Token, draft, _now);

        Record.Exception(send);

        Assert.Empty(_unitOfWork.MessageRepository.Messages);
    }

    [Fact]
    public void OpenConversation_Friend_ReturnsMessagesInOrder()
    {
        AddPlayers();
        AddMessage(_friendToCaller, FirstText);
        AddMessage(_callerToFriend, SecondText);
        string[] expectedTexts = { FirstText, SecondText };

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        IEnumerable<string> texts = conversation.Messages.Select(entry => entry.Message.Text);
        Assert.Equal(expectedTexts, texts);
    }

    [Fact]
    public void OpenConversation_OwnMessage_MarksItAsOwn()
    {
        AddPlayers();
        AddMessage(_callerToFriend, CleanText);

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        PrivateMessageEntry entry = conversation.Messages[0];
        Assert.True(entry.IsOwn);
    }

    [Fact]
    public void OpenConversation_UnreadMessage_MarksItRead()
    {
        AddPlayers();
        Message received = AddMessage(_friendToCaller, CleanText);

        _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.Equal(_now, received.ReadAt);
    }

    [Fact]
    public void OpenConversation_OwnUnreadMessage_StaysUnread()
    {
        AddPlayers();
        Message sent = AddMessage(_callerToFriend, CleanText);

        _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.Null(sent.ReadAt);
    }

    [Fact]
    public void OpenConversation_Friend_CanWrite()
    {
        AddPlayers();

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.True(conversation.Partner.IsFriend);
    }

    [Fact]
    public void OpenConversation_NonFriendWithHistory_ReturnsReadOnly()
    {
        AddPlayers();
        AddMessage(_strangerToCaller, StrangerText);

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, StrangerFriendCode, _now);

        Assert.False(conversation.Partner.IsFriend);
    }

    [Fact]
    public void OpenConversation_StrangerWithoutHistory_RejectsNotFriends()
    {
        AddPlayers();
        Action open = () => _privateChatService.OpenConversation(Token, StrangerFriendCode, _now);

        Exception? exception = Record.Exception(open);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.NotFriends, rejection?.Rejection);
    }

    [Fact]
    public void OpenConversation_ChatSanction_ReturnsMute()
    {
        AddPlayers();
        AddChatSanction(endsAt: null);

        PrivateConversation conversation = _privateChatService.OpenConversation(Token, FriendFriendCode, _now);

        Assert.True(conversation.Mute.IsMuted);
    }

    [Fact]
    public void ListConversations_TwoPartners_OrdersByLatestMessage()
    {
        AddPlayers();
        DateTime olderSentAt = _now.AddDays(-OlderMessageDays);
        AddMessage(_strangerToCaller, OldText, olderSentAt);
        AddMessage(_friendToCaller, NewText, _oneMinuteAgo);
        int[] expectedPartnerIds = { FriendAccountId, StrangerAccountId };

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        IEnumerable<int> partnerIds = conversations.Select(summary => summary.Partner.Player.AccountId);
        Assert.Equal(expectedPartnerIds, partnerIds);
    }

    [Fact]
    public void ListConversations_SeveralMessages_ShowsTheLatest()
    {
        AddPlayers();
        AddMessage(_friendToCaller, FirstText, _twoMinutesAgo);
        AddMessage(_callerToFriend, LastText, _oneMinuteAgo);

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        ConversationSummary conversation = conversations[0];
        Assert.Equal(LastText, conversation.LastMessage.Message.Text);
    }

    [Fact]
    public void ListConversations_UnreadMessages_CountsOnlyReceived()
    {
        AddPlayers();
        DateTime threeMinutesAgo = _now.AddMinutes(-ThreeMinutes);
        AddMessage(_friendToCaller, FirstUnreadText, threeMinutesAgo);
        AddMessage(_friendToCaller, SecondUnreadText, _twoMinutesAgo);
        AddMessage(_callerToFriend, OwnText, _oneMinuteAgo);

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        ConversationSummary conversation = conversations[0];
        Assert.Equal(TwoUnread, conversation.UnreadCount);
    }

    [Fact]
    public void ListConversations_NonFriendWithHistory_IsNotFriend()
    {
        AddPlayers();
        AddMessage(_strangerToCaller, CleanText);

        IReadOnlyList<ConversationSummary> conversations = _privateChatService.ListConversations(Token, _now);

        ConversationSummary conversation = conversations[0];
        Assert.False(conversation.Partner.IsFriend);
    }

    [Fact]
    public void MarkConversationRead_UnreadMessage_MarksItRead()
    {
        AddPlayers();
        Message received = AddMessage(_friendToCaller, CleanText);

        _privateChatService.MarkConversationRead(Token, FriendFriendCode, _now);

        Assert.Equal(_now, received.ReadAt);
    }

    private static PrivateMessageDraft CreateDraftToFriend(string text)
    {
        return new PrivateMessageDraft(FriendFriendCode, text);
    }

    private void AddPlayers()
    {
        _sessionVerifier.Add(Token, _caller);
        _unitOfWork.AccountRepository.AddPlayerIdentity(_caller);
        _unitOfWork.AccountRepository.AddPlayerIdentity(_friend);
        _unitOfWork.AccountRepository.AddPlayerIdentity(_stranger);
        var friendSummary = new FriendSummary(_friend, lastSeenAt: null);
        _unitOfWork.FriendshipRepository.AddFriendSummary(AccountId, friendSummary);
    }

    private void AddChatSanction(DateTime? endsAt)
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, endsAt);
        _unitOfWork.SanctionRepository.AddSanction(sanction);
    }

    private Message AddMessage(MessageRoute route, string text)
    {
        return AddMessage(route, text, _oneMinuteAgo);
    }

    private Message AddMessage(MessageRoute route, string text, DateTime sentAt)
    {
        var message = Message.CreatePrivate(route, text, sentAt);
        _unitOfWork.MessageRepository.Add(message);
        return message;
    }
}
