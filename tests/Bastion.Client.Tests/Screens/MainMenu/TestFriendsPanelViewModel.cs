using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu;

public class TestFriendsPanelViewModel
{
    private const string SessionToken = "session-token";
    private const string OwnNickname = "marta_muros";
    private const string PreferredLanguage = "es-MX";

    private readonly FakeNotificationClient _notifications = new FakeNotificationClient();

    [Fact]
    public void Constructor_Created_SubscribesToNotifications()
    {
        using FriendsPanelViewModel viewModel = CreateViewModel();

        Assert.True(_notifications.HasAnySubscribers());
    }

    [Fact]
    public void Dispose_Subscribed_RemovesEveryNotificationHandler()
    {
        FriendsPanelViewModel viewModel = CreateViewModel();

        viewModel.Dispose();

        Assert.False(_notifications.HasAnySubscribers());
    }

    private FriendsPanelViewModel CreateViewModel()
    {
        var session = new SignedInSession(SessionToken, OwnNickname, PreferredLanguage);
        var chatClients = new ChatClients(new FakeChatClient(), new FakeConversationClient(), _notifications);
        var clients = new SocialClients(new FakeFriendClient(), chatClients, _notifications);
        return InlineUiContext.CreateViewModel(() => new FriendsPanelViewModel(session, clients));
    }
}
