using System;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Screens.MainMenu;

public sealed record MainMenuServices
{
    public MainMenuServices(INotificationClient notifications, IMatchClient matches, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Notifications = notifications;
        Matches = matches;
        TimeProvider = timeProvider;
    }

    public INotificationClient Notifications { get; }

    public IMatchClient Matches { get; }

    public TimeProvider TimeProvider { get; }
}
