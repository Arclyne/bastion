using System;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Screens.MainMenu.Chat;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed record SocialClientSet
{
    public SocialClientSet(IFriendClient friends, ChatClientSet chat)
    {
        ArgumentNullException.ThrowIfNull(friends);
        ArgumentNullException.ThrowIfNull(chat);

        Friends = friends;
        Chat = chat;
    }

    public IFriendClient Friends { get; }

    public ChatClientSet Chat { get; }
}
