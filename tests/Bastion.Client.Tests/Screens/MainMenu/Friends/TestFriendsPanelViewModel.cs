using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu.Friends;

public sealed class TestFriendsPanelViewModel
{
    private const string SessionToken = "session-token";
    private const string OwnNickname = "marta_muros";
    private const string PreferredLanguage = "es-MX";

    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 12, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeNotificationClient _notifications = new FakeNotificationClient();
    private readonly FakeClipboard _clipboard = new FakeClipboard();
    private readonly LoggedInSession _session;
    private readonly SocialClientSet _clients;
    private readonly LocalServiceSet _localServices;

    public TestFriendsPanelViewModel()
    {
        _session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        var chatClient = new FakeChatClient();
        var conversationClient = new FakeConversationClient();
        var chatClients = new ChatClientSet(chatClient, conversationClient, _notifications);
        var friendClient = new FakeFriendClient();
        _clients = new SocialClientSet(friendClient, chatClients);
        var languages = new FakeLanguageService();
        var timeProvider = new ManualTimeProvider(_now);
        _localServices = new LocalServiceSet(languages, _clipboard, timeProvider);
    }

    [Fact]
    public void Constructor_Created_SubscribesToNotifications()
    {
        using FriendsPanelViewModel viewModel = InlineUiContext.CreateViewModel(
            () => new FriendsPanelViewModel(_session, _clients, _localServices));

        bool hasSubscribers = _notifications.HasAnySubscribers();
        Assert.True(hasSubscribers);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesEveryNotificationHandler()
    {
        FriendsPanelViewModel viewModel = CreateViewModel();

        viewModel.Dispose();

        bool hasSubscribers = _notifications.HasAnySubscribers();
        Assert.False(hasSubscribers);
    }

    [Fact]
    public void CopyOwnCode_ClipboardAvailable_MarksCodeCopied()
    {
        using FriendsPanelViewModel viewModel = CreateViewModel();

        viewModel.CopyOwnCode();

        Assert.True(viewModel.IsCodeCopied);
    }

    [Fact]
    public void CopyOwnCode_ClipboardUnavailable_LeavesCodeNotCopied()
    {
        using FriendsPanelViewModel viewModel = CreateViewModel();
        _clipboard.IsAvailable = false;

        viewModel.CopyOwnCode();

        Assert.False(viewModel.IsCodeCopied);
    }

    private FriendsPanelViewModel CreateViewModel()
    {
        return InlineUiContext.CreateViewModel(
            () => new FriendsPanelViewModel(_session, _clients, _localServices));
    }
}
