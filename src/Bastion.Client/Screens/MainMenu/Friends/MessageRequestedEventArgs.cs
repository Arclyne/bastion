using System;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class MessageRequestedEventArgs : EventArgs
{
    public MessageRequestedEventArgs(PlayerTag friend)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Friend = friend;
    }

    public PlayerTag Friend { get; }
}
