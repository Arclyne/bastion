using Bastion.Client.Networking.Accounts;

namespace Bastion.Client.Screens;

public interface INavigator
{
    void ShowLogin();

    void ShowMainMenu(SignedInSession session);
}
