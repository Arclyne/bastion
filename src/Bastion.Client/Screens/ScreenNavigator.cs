using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens.Login;
using Bastion.Client.Screens.MainMenu;
using Gum;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens;

public sealed class ScreenNavigator : INavigator
{
    private readonly ILanguageService _languages;
    private readonly IAccountClient _accounts;
    private FrameworkElement? _currentScreen;

    public ScreenNavigator(ILanguageService languages, IAccountClient accounts)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(accounts);

        _languages = languages;
        _accounts = accounts;
    }

    public void ShowLogin()
    {
        Show(new LoginView(new LoginViewModel(_languages, _accounts, this)));
    }

    public void ShowMainMenu(string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        Show(new MainMenuView(new MainMenuViewModel(nickname)));
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
