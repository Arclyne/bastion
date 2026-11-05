using System;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Networking.Notifications;

public sealed class FriendPresenceChangedEventArgs : EventArgs
{
    public FriendPresenceChangedEventArgs(ListedFriend friend, DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Friend = friend;
        OccurredAt = occurredAt;
    }

    public ListedFriend Friend { get; }

    public DateTime OccurredAt { get; }
}
