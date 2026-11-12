using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Friends;

public sealed record FriendRequestListing
{
    public FriendRequestListing(
        IReadOnlyList<PendingFriendRequest> receivedRequests,
        IReadOnlyList<PendingFriendRequest> sentRequests,
        DateTime listedAt)
    {
        ArgumentNullException.ThrowIfNull(receivedRequests);
        ArgumentNullException.ThrowIfNull(sentRequests);

        ReceivedRequests = receivedRequests;
        SentRequests = sentRequests;
        ListedAt = listedAt;
    }

    public IReadOnlyList<PendingFriendRequest> ReceivedRequests { get; }

    public IReadOnlyList<PendingFriendRequest> SentRequests { get; }

    public DateTime ListedAt { get; }
}
