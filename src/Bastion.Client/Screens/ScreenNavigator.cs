using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens.Login;
using Bastion.Client.Screens.MainMenu;
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
        Show(new LoginView(new LoginViewModel(_languages, _connection.Accounts, this)));
    }

    public void ShowMainMenu(SignedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var viewModel = new MainMenuViewModel(session, _connection.Notifications, _connection.Chat);
        Show(new MainMenuView(viewModel));
        _ = viewModel.ConnectChat();
    }

    private void Show(FrameworkElement screen)
    {
        if (_currentScreen is not null)
        {
            GumService.Default.Root.Children.Remove(_currentScreen.Visual);
        }

        _currentScreen = screen;
        GumService.Default.Root.AddChild(screen);
    }
}
