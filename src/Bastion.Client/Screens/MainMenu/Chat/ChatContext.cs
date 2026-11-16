using System;
using Bastion.Client.Networking.Accounts;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed record ChatContext
{
    public ChatContext(LoggedInSession session, ChatClientSet clients, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(clients);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Session = session;
        Clients = clients;
        TimeProvider = timeProvider;
    }

    public LoggedInSession Session { get; }

    public ChatClientSet Clients { get; }

    public TimeProvider TimeProvider { get; }
}
