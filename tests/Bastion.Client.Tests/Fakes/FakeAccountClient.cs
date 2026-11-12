using System;
using Bastion.Client.Networking.Accounts;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeAccountClient : IAccountClient
{
    private const string SessionToken = "session-token";
    private const string Nickname = "marta_muros";

    public int LogInCount { get; private set; }

    public LoggedInSession LogIn(LoginCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        LogInCount++;
        return new LoggedInSession(SessionToken, Nickname, string.Empty);
    }
}
