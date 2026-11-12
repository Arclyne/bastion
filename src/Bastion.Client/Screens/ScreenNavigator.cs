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

namespace Bastion.Client.Screens;

public sealed class ScreenNavigator : INavigator, IDisposable
{
    private readonly ServerConnection _connection;
    private readonly ILanguageService _languages;
    private readonly LocalServices _localServices;
    private readonly TimeProvider _timeProvider;
    private readonly IScreenHost _screenHost;
    private readonly List<IDisposable> _screenViewModels = new List<IDisposable>();
    private ScreenView? _currentScreen;
    private LoggedInSession? _loggedInSession;
    private JoinedMatch? _joinedMatch;
    private MatchChatViewModel? _matchChatViewModel;
    private MatchAbandonmentViewModel? _matchAbandonmentViewModel;

    public ScreenNavigator(ServerConnection connection, LocalServices localServices, IScreenHost screenHost)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(localServices);
        ArgumentNullException.ThrowIfNull(screenHost);

        _connection = connection;
        _languages = localServices.Languages;
        _localServices = localServices;
        _timeProvider = localServices.TimeProvider;
        _screenHost = screenHost;
    }

    public MatchViewModel? ShownMatch { get; private set; }

    public void ShowLogin()
    {
        var loginViewModel = new LoginViewModel(_languages, _connection.Accounts, this);
        ShowScreen(new LoginView(loginViewModel, _languages));
    }

    public void ShowMainMenu(LoggedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        MainMenuViewModel mainMenuViewModel = OpenMainMenu(session);
        _ = mainMenuViewModel.ConnectNotifications();
    }

    public void ReceiveEscapeKey()
    {
        if (_currentScreen is not IEscapeKeyReceiver receiver)
        {
            return;
        }

        receiver.ReceiveEscapeKey();
    }

    public void Dispose()
    {
        DisposeViewModelsExcept(Array.Empty<IDisposable>());
    }

    private MainMenuViewModel OpenMainMenu(LoggedInSession session)
    {
        var mainMenuViewModel = new MainMenuViewModel(session, _connection.Notifications);
        var chatClients = new ChatClients(_connection.Chat, _connection.Conversations, _connection.Notifications);
        var clients = new SocialClients(_connection.Friends, chatClients, _connection.Notifications);
        var friendsPanel = new FriendsPanelViewModel(session, clients, _localServices);
        var matchSearchViewModel = new MatchSearchViewModel(session, _connection.Matchmaking, _timeProvider);
        matchSearchViewModel.MatchFound += HandleMatchFound;
        ShowScreen(
            new MainMenuView(mainMenuViewModel, friendsPanel, matchSearchViewModel),
            friendsPanel,
            matchSearchViewModel);
        _loggedInSession = session;
        _ = friendsPanel.LoadFriends();
        _ = friendsPanel.Requests.LoadRequests();
        _ = friendsPanel.Chat.LoadConversations();
        return mainMenuViewModel;
    }

    private void ShowVersus(JoinedMatch joinedMatch)
    {
        var matchChatViewModel = new MatchChatViewModel(joinedMatch, _connection.Chat, _timeProvider);
        var matchAbandonmentViewModel = new MatchAbandonmentViewModel(joinedMatch, _connection.Matches);
        matchAbandonmentViewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        var versusViewModel = new VersusViewModel(joinedMatch, _timeProvider);
        versusViewModel.CountdownFinished += HandleCountdownFinished;
        ShowScreen(
            new VersusView(versusViewModel),
            versusViewModel,
            matchChatViewModel,
            matchAbandonmentViewModel);
        _joinedMatch = joinedMatch;
        _matchChatViewModel = matchChatViewModel;
        _matchAbandonmentViewModel = matchAbandonmentViewModel;
    }

    private void ShowMatch(
        JoinedMatch joinedMatch,
        MatchChatViewModel matchChatViewModel,
        MatchAbandonmentViewModel matchAbandonmentViewModel)
    {
        var matchViewModel = new MatchViewModel(joinedMatch, matchAbandonmentViewModel);
        ShowScreen(
            new MatchView(matchViewModel, matchChatViewModel, matchAbandonmentViewModel),
            matchChatViewModel,
            matchAbandonmentViewModel,
            matchViewModel);
        ShownMatch = matchViewModel;
    }

    private void ShowScreen(ScreenView screen, params IDisposable[] screenViewModels)
    {
        ShownMatch = null;
        _screenHost.ShowScreen(screen);
        _currentScreen = screen;
        DisposeViewModelsExcept(screenViewModels);
        _screenViewModels.AddRange(screenViewModels);
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
        if (_loggedInSession is null)
        {
            return;
        }

        ShowVersus(new JoinedMatch(_loggedInSession, e));
    }

    private void HandleCountdownFinished(object? sender, EventArgs e)
    {
        if (_joinedMatch is null || _matchChatViewModel is null || _matchAbandonmentViewModel is null)
        {
            return;
        }

        ShowMatch(_joinedMatch, _matchChatViewModel, _matchAbandonmentViewModel);
    }

    private void HandleReturnToMenuRequested(object? sender, EventArgs e)
    {
        if (_loggedInSession is null)
        {
            return;
        }

        OpenMainMenu(_loggedInSession);
    }
}
