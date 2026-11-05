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

public sealed class ScreenNavigator : INavigator, IDisposable
{
    private readonly ILanguageService _languages;
    private readonly ServerConnection _connection;
    private readonly TimeProvider _timeProvider;
    private FrameworkElement? _currentScreen;
    private MatchSearchViewModel? _matchSearchViewModel;

    public ScreenNavigator(ILanguageService languages, ServerConnection connection, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _languages = languages;
        _connection = connection;
        _timeProvider = timeProvider;
    }

    public void ShowLogin()
    {
        ShowScreen(new LoginView(new LoginViewModel(_languages, _connection.Accounts, this)));
    }

    public void ShowMainMenu(SignedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var mainMenuViewModel = new MainMenuViewModel(session, _connection.Notifications);
        var chatClients = new ChatClients(_connection.Chat, _connection.Conversations, _connection.Notifications);
        var clients = new SocialClients(_connection.Friends, chatClients, _connection.Notifications);
        var friendsPanel = new FriendsPanelViewModel(session, clients);
        var matchSearchViewModel = new MatchSearchViewModel(session, _connection.Matchmaking, _timeProvider);
        ShowScreen(new MainMenuView(mainMenuViewModel, friendsPanel, matchSearchViewModel));
        _matchSearchViewModel = matchSearchViewModel;
        _ = mainMenuViewModel.ConnectNotifications();
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

    public void Dispose()
    {
        DisposeMatchSearchViewModel();
    }

    private void ShowScreen(FrameworkElement screen)
    {
        if (_currentScreen is not null)
        {
            GumService.Default.Root.Children.Remove(_currentScreen.Visual);
        }

        DisposeMatchSearchViewModel();
        _currentScreen = screen;
        GumService.Default.Root.AddChild(screen);
    }

    private void DisposeMatchSearchViewModel()
    {
        _matchSearchViewModel?.Dispose();
        _matchSearchViewModel = null;
    }
}
