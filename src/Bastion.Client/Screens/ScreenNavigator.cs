using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Login;
using Bastion.Client.Screens.MainMenu;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Screens.Matches;
using Gum;
using Gum.Forms.Controls;
using Gum.Wireframe;

namespace Bastion.Client.Screens;

public sealed class ScreenNavigator : INavigator, IDisposable
{
    private readonly ILanguageService _languages;
    private readonly ServerConnection _connection;
    private readonly TimeProvider _timeProvider;
    private readonly List<IDisposable> _screenViewModels = new List<IDisposable>();
    private FrameworkElement? _currentScreen;
    private SignedInSession? _signedInSession;
    private JoinedMatch? _joinedMatch;
    private MatchChatViewModel? _matchChatViewModel;

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
        matchSearchViewModel.MatchFound += HandleMatchFound;
        var mainMenuView = new MainMenuView(mainMenuViewModel, friendsPanel, matchSearchViewModel);
        ShowScreen(mainMenuView, friendsPanel, matchSearchViewModel);
        _signedInSession = session;
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
        DisposeViewModelsExcept(Array.Empty<IDisposable>());
    }

    private void ShowVersus(JoinedMatch joinedMatch)
    {
        // The match chat listens from the VS screen on, so a rival whose countdown ends first is not missed.
        var matchChatViewModel = new MatchChatViewModel(joinedMatch, _connection.Chat, _timeProvider);
        var versusViewModel = new VersusViewModel(joinedMatch, _timeProvider);
        versusViewModel.CountdownFinished += HandleCountdownFinished;
        ShowScreen(new VersusView(versusViewModel), versusViewModel, matchChatViewModel);
        _joinedMatch = joinedMatch;
        _matchChatViewModel = matchChatViewModel;
    }

    private void ShowMatch(JoinedMatch joinedMatch, MatchChatViewModel matchChatViewModel)
    {
        ShowScreen(new MatchView(new MatchViewModel(joinedMatch), matchChatViewModel), matchChatViewModel);
    }

    private void ShowScreen(FrameworkElement screen, params IDisposable[] nextScreenViewModels)
    {
        if (_currentScreen is not null)
        {
            GumService.Default.Root.Children.Remove(_currentScreen.Visual);
        }

        // A dialog or a text field of the screen being left must not keep the input of the next one.
        GumService.Default.ModalRoot.Children.Clear();
        InteractiveGue.CurrentInputReceiver = null;
        DisposeViewModelsExcept(nextScreenViewModels);
        _screenViewModels.AddRange(nextScreenViewModels);
        _currentScreen = screen;
        GumService.Default.Root.AddChild(screen);
    }

    private void DisposeViewModelsExcept(IReadOnlyCollection<IDisposable> keptViewModels)
    {
        IEnumerable<IDisposable> leftViewModels = _screenViewModels.Except(keptViewModels);
        foreach (IDisposable viewModel in leftViewModels)
        {
            viewModel.Dispose();
        }

        _screenViewModels.Clear();
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        if (_signedInSession is null)
        {
            return;
        }

        ShowVersus(new JoinedMatch(_signedInSession, e));
    }

    private void HandleCountdownFinished(object? sender, EventArgs e)
    {
        if (_joinedMatch is null || _matchChatViewModel is null)
        {
            return;
        }

        ShowMatch(_joinedMatch, _matchChatViewModel);
    }
}
