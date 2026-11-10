using System;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public sealed record FriendSummary
{
    public FriendSummary(PlayerIdentity friend, DateTime? lastSeenAt)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Friend = friend;
        LastSeenAt = lastSeenAt;
    }

    public PlayerIdentity Friend { get; }

    public DateTime? LastSeenAt { get; }
}
