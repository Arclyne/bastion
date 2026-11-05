using System;

namespace Bastion.Client.Networking.Friends;

public sealed class FriendshipRejectedException : Exception
{
    public FriendshipRejectedException(FriendshipRejection rejection, Exception innerException)
        : base($"The friendship operation was rejected. Rejection={rejection}", innerException)
    {
        Rejection = rejection;
    }

    public FriendshipRejection Rejection { get; }
}
