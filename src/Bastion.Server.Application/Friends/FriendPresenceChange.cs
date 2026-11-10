using System;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public sealed record FriendPresenceChange
{
    public FriendPresenceChange(PlayerIdentity friend, FriendPresence presence, DateTime? lastSeenAt)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Friend = friend;
        Presence = presence;
        LastSeenAt = lastSeenAt;
    }

    public PlayerIdentity Friend { get; }

    public FriendPresence Presence { get; }

    public DateTime? LastSeenAt { get; }
}
