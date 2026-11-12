using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeNavigator : INavigator
{
    public void ShowLogin()
    {
    }

    public void ShowMainMenu(LoggedInSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
    }
}
