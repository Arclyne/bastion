using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens.Login;
using Bastion.Client.Screens.MainMenu;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Gum;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens;

public sealed class ScreenNavigator : INavigator
{
    private readonly ILanguageService _languages;
    private readonly ServerConnection _connection;
    private FrameworkElement? _currentScreen;

    public ScreenNavigator(ILanguageService languages, ServerConnection connection)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(connection);

        _languages = languages;
        _connection = connection;
    }

    public void ShowLogin()
    {
        ShowScreen(new LoginView(new LoginViewModel(_languages, _connection.Accounts, this)));
    }

    public void ShowMainMenu(SignedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var viewModel = new MainMenuViewModel(session, _connection.Notifications);
        var chatClients = new ChatClients(_connection.Chat, _connection.Conversations, _connection.Notifications);
        var clients = new SocialClients(_connection.Friends, chatClients, _connection.Notifications);
        var friendsPanel = new FriendsPanelViewModel(session, clients);
        ShowScreen(new MainMenuView(viewModel, friendsPanel));
        _ = viewModel.ConnectNotifications();
        _ = friendsPanel.LoadFriends();
        _ = friendsPanel.Requests.LoadRequests();
        _ = friendsPanel.Chat.LoadConversations();
    }

    public void ReceiveEscapeKey()
    {
        if (_currentScreen is IEscapeKeyReceiver receiver)
        {
            receiver.ReceiveEscapeKey();
        }
    }

    private void ShowScreen(FrameworkElement screen)
    {
        if (_currentScreen is not null)
        {
            GumService.Default.Root.Children.Remove(_currentScreen.Visual);
        }

        _currentScreen = screen;
        GumService.Default.Root.AddChild(screen);
    }
}
