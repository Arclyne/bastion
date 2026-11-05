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
    private MatchAbandonmentViewModel? _matchAbandonmentViewModel;

    public ScreenNavigator(ILanguageService languages, ServerConnection connection, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _languages = languages;
        _connection = connection;
        _timeProvider = timeProvider;
    }

    public MatchViewModel? ShownMatch { get; private set; }

    public void ShowLogin()
    {
        ShowScreen(() => new LoginView(new LoginViewModel(_languages, _connection.Accounts, this)));
    }

    public void ShowMainMenu(SignedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        MainMenuViewModel mainMenuViewModel = OpenMainMenu(session);
        _ = mainMenuViewModel.ConnectNotifications();
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

    private MainMenuViewModel OpenMainMenu(SignedInSession session)
    {
        var mainMenuViewModel = new MainMenuViewModel(session, _connection.Notifications);
        var chatClients = new ChatClients(_connection.Chat, _connection.Conversations, _connection.Notifications);
        var clients = new SocialClients(_connection.Friends, chatClients, _connection.Notifications);
        var friendsPanel = new FriendsPanelViewModel(session, clients);
        var matchSearchViewModel = new MatchSearchViewModel(session, _connection.Matchmaking, _timeProvider);
        matchSearchViewModel.MatchFound += HandleMatchFound;
        ShowScreen(
            () => new MainMenuView(mainMenuViewModel, friendsPanel, matchSearchViewModel),
            friendsPanel,
            matchSearchViewModel);
        _signedInSession = session;
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
            () => new VersusView(versusViewModel),
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
            () => new MatchView(matchViewModel, matchChatViewModel, matchAbandonmentViewModel),
            matchChatViewModel,
            matchAbandonmentViewModel,
            matchViewModel);
        ShownMatch = matchViewModel;
    }

    private void ShowScreen(Func<FrameworkElement> createScreen, params IDisposable[] nextScreenViewModels)
    {
        ShownMatch = null;
        if (_currentScreen is not null)
        {
            GumService.Default.Root.Children.Remove(_currentScreen.Visual);
        }

        GumService.Default.ModalRoot.Children.Clear();
        InteractiveGue.CurrentInputReceiver = null;
        DisposeViewModelsExcept(nextScreenViewModels);
        _screenViewModels.AddRange(nextScreenViewModels);
        _currentScreen = createScreen();
        GumService.Default.Root.AddChild(_currentScreen);
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
        if (_joinedMatch is null || _matchChatViewModel is null || _matchAbandonmentViewModel is null)
        {
            return;
        }

        ShowMatch(_joinedMatch, _matchChatViewModel, _matchAbandonmentViewModel);
    }

    private void HandleReturnToMenuRequested(object? sender, EventArgs e)
    {
        if (_signedInSession is null)
        {
            return;
        }

        OpenMainMenu(_signedInSession);
    }
}
