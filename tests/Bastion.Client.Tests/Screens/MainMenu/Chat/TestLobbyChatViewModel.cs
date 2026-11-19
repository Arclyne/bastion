using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Sessions;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu.Chat;

public sealed class TestLobbyChatViewModel : IDisposable
{
    private const string SessionToken = "lobby-session-token";
    private const string OwnNickname = "Angel";
    private const string PreferredLanguage = "es-MX";
    private const string MessageText = "¿Alguien para una partida?";
    private const string RejectionCause = "The server turned the message down.";

    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 5, 18, 1, 0, TimeSpan.Zero);
    private static readonly DateTime _mutedUntil = new DateTime(2026, 11, 5, 19, 0, 0, DateTimeKind.Utc);

    private readonly FakeChatClient _chatClient = new FakeChatClient();
    private readonly LobbyChatViewModel _viewModel;

    public TestLobbyChatViewModel()
    {
        var session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        var conversationClient = new FakeConversationClient();
        var notificationClient = new FakeNotificationClient();
        var clients = new ChatClientSet(_chatClient, conversationClient, notificationClient);
        var timeProvider = new ManualTimeProvider(_now);
        var context = new ChatContext(session, clients, timeProvider);
        Func<LobbyChatViewModel> createViewModel = () => new LobbyChatViewModel(context);
        _viewModel = InlineUiContext.CreateViewModel(createViewModel);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public async Task SendMessage_ValidDraft_SendsDraftText()
    {
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.Equal(MessageText, _chatClient.LastSentText);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_ConfirmsOwnRow()
    {
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.False(ownRow.IsPending);
    }

    [Fact]
    public async Task SendMessage_Muted_ShowsRejectionOnRow()
    {
        _chatClient.LobbyChatSendFailure = CreateRejection(ChatRejection.Muted);
        _viewModel.Draft = MessageText;
        string expectedState = ChatRejectionText.Describe(ChatRejection.Muted);

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(expectedState, ownRow.StateText);
    }

    [Fact]
    public async Task SendMessage_Muted_MutesComposer()
    {
        _chatClient.LobbyChatSendFailure = CreateRejection(ChatRejection.Muted);
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.True(_viewModel.IsMuted);
    }

    [Fact]
    public async Task SendMessage_InvalidSession_ShowsSendFailedOnRow()
    {
        _chatClient.LobbyChatSendFailure = new InvalidSessionException();
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(Strings.ChatSendFailed, ownRow.StateText);
    }

    [Theory]
    [InlineData(typeof(ServerUnreachableException))]
    [InlineData(typeof(ConnectionLostException))]
    [InlineData(typeof(ServerErrorException))]
    public async Task SendMessage_ServiceFailure_ShowsSendFailedOnRow(Type failureType)
    {
        _chatClient.LobbyChatSendFailure = CreateServiceFailure(failureType);
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(Strings.ChatSendFailed, ownRow.StateText);
    }

    [Fact]
    public async Task SendMessage_ServiceFailure_AllowsSendingAgain()
    {
        _chatClient.LobbyChatSendFailure = new ServerErrorException();
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.True(_viewModel.CanSend);
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
