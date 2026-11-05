using System;

namespace Bastion.Server.Application.Friends;

public sealed class FriendshipRejectedException : Exception
{
    public FriendshipRejectedException(FriendshipRejection rejection)
        : base($"The friendship operation was rejected. Rejection={rejection}")
    {
        Rejection = rejection;
    }

    public FriendshipRejection Rejection { get; }
}
