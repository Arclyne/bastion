using System;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuViewModel : ViewModel
{
    public MainMenuViewModel(string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        Nickname = nickname;
    }

    public string Nickname
    {
        get => Get<string>();
        private set => Set(value);
    }
}
