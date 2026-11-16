using Bastion.Client.Networking.Accounts;

namespace Bastion.Client.Screens;

public interface INavigator
{
    void ShowMainMenu(LoggedInSession session);
}
