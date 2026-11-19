using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Sessions;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu.Chat;

public sealed class TestPrivateConversationViewModel
{
    private const string SessionToken = "private-session-token";
    private const string OwnNickname = "Angel";
    private const string PreferredLanguage = "es-MX";
    private const string PartnerNickname = "José";
    private const string PartnerFriendCode = "JOSE-4821";
    private const string PresenceStatus = "En línea";
    private const string MessageText = "¿Jugamos una partida rápida?";
    private const string RejectionCause = "The server turned the message down.";

    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 12, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTime _mutedUntil = new DateTime(2026, 11, 12, 11, 0, 0, DateTimeKind.Utc);

    private readonly FakeConversationClient _conversationClient = new FakeConversationClient();
    private readonly PrivateConversationViewModel _viewModel;
    private PrivateMessage? _sentMessage;

    public TestPrivateConversationViewModel()
    {
        var session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        var chatClient = new FakeChatClient();
        var notificationClient = new FakeNotificationClient();
        var clients = new ChatClientSet(chatClient, _conversationClient, notificationClient);
        var timeProvider = new ManualTimeProvider(_now);
        var context = new ChatContext(session, clients, timeProvider);
        _viewModel = new PrivateConversationViewModel(context);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_SendsToPartner()
    {
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        Assert.Equal(PartnerFriendCode, _conversationClient.LastSentFriendCode);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_ConfirmsOwnRow()
    {
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        PrivateMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.False(ownRow.IsPending);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_RaisesMessageSentWithConfirmedTime()
    {
        await OpenConversationWithDraft();
        _viewModel.MessageSent += HandleMessageSent;
        var expectedMessage = new PrivateMessage(MessageText, FakeConversationClient.ConfirmedSentAt, isOwn: true);

        await _viewModel.SendMessage();

        Assert.Equal(expectedMessage, _sentMessage);
    }

    [Fact]
    public async Task SendMessage_Rejected_RaisesNoMessageSent()
    {
        _conversationClient.SendFailure = CreateRejection(ChatRejection.TooFast);
        await OpenConversationWithDraft();
        _viewModel.MessageSent += HandleMessageSent;

        await _viewModel.SendMessage();

        Assert.Null(_sentMessage);
    }

    [Fact]
    public async Task SendMessage_NotFriends_ShowsNotFriendsNotice()
    {
        _conversationClient.SendFailure = CreateRejection(ChatRejection.NotFriends);
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        Assert.True(_viewModel.IsNotFriendsNoticeVisible);
    }

    [Fact]
    public async Task SendMessage_Muted_MutesComposer()
    {
        _conversationClient.SendFailure = CreateRejection(ChatRejection.Muted);
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        Assert.True(_viewModel.IsMuted);
    }

    [Fact]
    public async Task SendMessage_InvalidSession_ShowsSendFailedOnRow()
    {
        _conversationClient.SendFailure = new InvalidSessionException();
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        PrivateMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(Strings.ChatSendFailed, ownRow.StateText);
    }

    [Theory]
    [InlineData(typeof(ServerUnreachableException))]
    [InlineData(typeof(ConnectionLostException))]
    [InlineData(typeof(ServerErrorException))]
    public async Task SendMessage_ServiceFailure_ShowsSendFailedOnRow(Type failureType)
    {
        _conversationClient.SendFailure = CreateServiceFailure(failureType);
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        PrivateMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(Strings.ChatSendFailed, ownRow.StateText);
    }

    [Fact]
    public async Task SendMessage_ServiceFailure_AllowsSendingAgain()
    {
        _conversationClient.SendFailure = new ServerErrorException();
        await OpenConversationWithDraft();

        await _viewModel.SendMessage();

        Assert.True(_viewModel.CanSend);
    }

    private void HandleMessageSent(object? sender, PrivateMessageSentEventArgs e)
    {
        _sentMessage = e.Message;
    }

    private async Task OpenConversationWithDraft()
    {
        var partner = new PlayerTag(PartnerNickname, PartnerFriendCode);
        var presence = new ConversationPresence(PartnerPresence.Connected, PresenceStatus);
        await _viewModel.Open(partner, presence);
        _viewModel.Draft = MessageText;
    }

    private static ChatMessageRejectedException CreateRejection(ChatRejection rejection)
    {
        var cause = new InvalidOperationException(RejectionCause);
        return new ChatMessageRejectedException(rejection, _mutedUntil, cause);
    }

    private static Exception CreateServiceFailure(Type failureType)
    {
        object? failure = Activator.CreateInstance(failureType);
        if (failure is not Exception serviceFailure)
        {
            throw new ArgumentOutOfRangeException(nameof(failureType));
        }

        return serviceFailure;
    }
}
