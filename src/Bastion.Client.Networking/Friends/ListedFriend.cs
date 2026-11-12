using System;

namespace Bastion.Client.Networking.Friends;

public sealed record ListedFriend
{
    public ListedFriend(PlayerTag friend, FriendPresence presence, DateTime? lastSeenAt)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Friend = friend;
        Presence = presence;
        LastSeenAt = lastSeenAt;
    }

    public PlayerTag Friend { get; }

    public FriendPresence Presence { get; }

    public DateTime? LastSeenAt { get; }
}
