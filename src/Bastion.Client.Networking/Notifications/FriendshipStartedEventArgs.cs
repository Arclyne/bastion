using System;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Networking.Notifications;

public sealed class FriendshipStartedEventArgs : EventArgs
{
    public FriendshipStartedEventArgs(PlayerTag friend)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Friend = friend;
    }

    public PlayerTag Friend { get; }
}
