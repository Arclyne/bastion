using System;

namespace Bastion.Client.Networking.Friends;

public sealed record PendingFriendRequest
{
    public PendingFriendRequest(PlayerTag player, DateTime requestedAt, DateTime expiresAt)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        RequestedAt = requestedAt;
        ExpiresAt = expiresAt;
    }

    public PlayerTag Player { get; }

    public DateTime RequestedAt { get; }

    public DateTime ExpiresAt { get; }
}
