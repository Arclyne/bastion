using System;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRequestSummary
{
    public FriendRequestSummary(PlayerIdentity counterpart, DateTime requestedAt, DateTime expiresAt)
    {
        ArgumentNullException.ThrowIfNull(counterpart);

        Counterpart = counterpart;
        RequestedAt = requestedAt;
        ExpiresAt = expiresAt;
    }

    public PlayerIdentity Counterpart { get; }

    public DateTime RequestedAt { get; }

    public DateTime ExpiresAt { get; }
}
