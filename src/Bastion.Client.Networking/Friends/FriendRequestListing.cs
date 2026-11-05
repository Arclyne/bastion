using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Friends;

public sealed record FriendRequestListing(
    IReadOnlyList<PendingFriendRequest> Received,
    IReadOnlyList<PendingFriendRequest> Sent,
    DateTime ListedAt);
