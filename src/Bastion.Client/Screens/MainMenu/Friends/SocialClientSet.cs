using System;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.MainMenu.Chat;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed record SocialClientSet
{
    public SocialClientSet(IFriendClient friends, ChatClientSet chat, INotificationClient notifications)
    {
        ArgumentNullException.ThrowIfNull(friends);
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(notifications);

        Friends = friends;
        Chat = chat;
        Notifications = notifications;
    }

    public IFriendClient Friends { get; }

    public ChatClientSet Chat { get; }

    public INotificationClient Notifications { get; }
}
