using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bastion.Client.Networking;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Login;
using Bastion.Client.Screens.MainMenu;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Screens.Matches;
using log4net;

namespace Bastion.Client.Screens;

public sealed class Navigator : INavigator, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Navigator));

    private readonly ServerConnection _connection;
    private readonly LocalServiceSet _localServices;
    private readonly IScreenHost _screenHost;
    private readonly List<IDisposable> _screenViewModels;
    private ScreenView? _currentScreen;
    private LoggedInSession? _loggedInSession;
    private JoinedMatch? _joinedMatch;
    private JoinedMatchViewModelSet? _joinedMatchViewModels;

    public Navigator(ServerConnection connection, LocalServiceSet localServices, IScreenHost screenHost)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(localServices);
        ArgumentNullException.ThrowIfNull(screenHost);

        _connection = connection;
        _localServices = localServices;
        _screenHost = screenHost;
        _screenViewModels = new List<IDisposable>();
    }

    public MatchViewModel? ShownMatch { get; private set; }

    public void ShowLogin()
    {
        var loginViewModel = new LoginViewModel(_localServices.Languages, _connection.Accounts, this);
        var loginView = new LoginView(loginViewModel, _localServices.Languages);
        ShowScreen(loginView);
    }

    public void ShowMainMenu(LoggedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        MainMenuViewModel mainMenuViewModel = OpenMainMenu(session);
        _ = RunSafely(mainMenuViewModel.ConnectNotifications());
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

    private MainMenuViewModel OpenMainMenu(LoggedInSession session)
    {
        var mainMenuViewModel = new MainMenuViewModel(session, _connection.Notifications);
        var chatClients = new ChatClientSet(_connection.Chat, _connection.Conversations, _connection.Notifications);
        var clients = new SocialClientSet(_connection.Friends, chatClients);
        var friendsPanel = new FriendsPanelViewModel(session, clients, _localServices);
        var matchSearchViewModel = new MatchSearchViewModel(
            session,
            _connection.Matchmaking,
            _localServices.TimeProvider);
        matchSearchViewModel.MatchFound += HandleMatchFound;
        var mainMenuView = new MainMenuView(mainMenuViewModel, friendsPanel, matchSearchViewModel);
        ShowScreen(mainMenuView, friendsPanel, matchSearchViewModel);
        _loggedInSession = session;
        _ = RunSafely(friendsPanel.LoadFriends());
        _ = RunSafely(friendsPanel.Requests.LoadRequests());
        _ = RunSafely(friendsPanel.Chat.LoadConversations());
        return mainMenuViewModel;
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        if (_loggedInSession is null)
        {
            return;
        }

        var joinedMatch = new JoinedMatch(_loggedInSession, e);
        ShowVersus(joinedMatch);
    }

    private void ShowVersus(JoinedMatch joinedMatch)
    {
        var matchChatViewModel = new MatchChatViewModel(joinedMatch, _connection.Chat, _localServices.TimeProvider);
        var matchAbandonmentViewModel = new MatchAbandonmentViewModel(joinedMatch, _connection.Matches);
        matchAbandonmentViewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        var matchPlayViewModel = new MatchPlayViewModel(joinedMatch, _connection.Matches, _localServices.TimeProvider);
        var versusViewModel = new VersusViewModel(joinedMatch, _localServices.TimeProvider);
        versusViewModel.CountdownFinished += HandleCountdownFinished;
        var versusView = new VersusView(versusViewModel);
        ShowScreen(versusView, versusViewModel, matchChatViewModel, matchAbandonmentViewModel, matchPlayViewModel);
        _joinedMatch = joinedMatch;
        _joinedMatchViewModels = new JoinedMatchViewModelSet(
            matchChatViewModel,
            matchAbandonmentViewModel,
            matchPlayViewModel);
    }

    private void HandleReturnToMenuRequested(object? sender, EventArgs e)
    {
        if (_loggedInSession is null)
        {
            return;
        }

        OpenMainMenu(_loggedInSession);
    }

    private void HandleCountdownFinished(object? sender, EventArgs e)
    {
        if (_joinedMatch is null || _joinedMatchViewModels is null)
        {
            return;
        }

        ShowMatch(_joinedMatch, _joinedMatchViewModels);
    }

    private void ShowMatch(JoinedMatch joinedMatch, JoinedMatchViewModelSet joinedMatchViewModels)
    {
        var matchViewModel = new MatchViewModel(
            joinedMatch,
            joinedMatchViewModels.Abandonment,
            joinedMatchViewModels.MatchPlay);
        var matchView = new MatchView(matchViewModel, joinedMatchViewModels.Chat, joinedMatchViewModels.Abandonment);
        ShowScreen(
            matchView,
            joinedMatchViewModels.Chat,
            joinedMatchViewModels.Abandonment,
            joinedMatchViewModels.MatchPlay,
            matchViewModel);
        ShownMatch = matchViewModel;
    }

    private static async Task RunSafely(Task task)
    {
        try
        {
            await task;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("A screen task ended after the screen was closed.", ex);
        }
    }
}
