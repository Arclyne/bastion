using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRoster
{
    public FriendRoster(string ownFriendCode, IReadOnlyList<FriendStatus> friends, DateTime listedAt)
    {
        ArgumentNullException.ThrowIfNull(ownFriendCode);
        ArgumentNullException.ThrowIfNull(friends);

        OwnFriendCode = ownFriendCode;
        Friends = friends;
        ListedAt = listedAt;
    }

    public string OwnFriendCode { get; }

    public IReadOnlyList<FriendStatus> Friends { get; }

    public DateTime ListedAt { get; }
}
