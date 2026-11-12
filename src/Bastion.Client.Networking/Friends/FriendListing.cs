using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Friends;

public sealed record FriendListing
{
    public FriendListing(string ownFriendCode, IReadOnlyList<ListedFriend> friends, DateTime listedAt)
    {
        ArgumentNullException.ThrowIfNull(ownFriendCode);
        ArgumentNullException.ThrowIfNull(friends);

        OwnFriendCode = ownFriendCode;
        Friends = friends;
        ListedAt = listedAt;
    }

    public string OwnFriendCode { get; }

    public IReadOnlyList<ListedFriend> Friends { get; }

    public DateTime ListedAt { get; }
}
