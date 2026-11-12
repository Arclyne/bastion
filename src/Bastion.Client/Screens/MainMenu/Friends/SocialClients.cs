using System;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.MainMenu.Chat;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed record SocialClients
{
    public SocialClients(IFriendClient friends, ChatClients chat, INotificationClient notifications)
    {
        ArgumentNullException.ThrowIfNull(friends);
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(notifications);

        Friends = friends;
        Chat = chat;
        Notifications = notifications;
    }

    public IFriendClient Friends { get; }

    public ChatClients Chat { get; }

    public INotificationClient Notifications { get; }
}
