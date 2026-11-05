using System;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Networking.Notifications;

public sealed class FriendRequestReceivedEventArgs : EventArgs
{
    public FriendRequestReceivedEventArgs(PendingFriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Request = request;
    }

    public PendingFriendRequest Request { get; }
}
