using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRequestBoard
{
    public FriendRequestBoard(
        IReadOnlyList<FriendRequestSummary> receivedRequests,
        IReadOnlyList<FriendRequestSummary> sentRequests,
        DateTime listedAt)
    {
        ArgumentNullException.ThrowIfNull(receivedRequests);
        ArgumentNullException.ThrowIfNull(sentRequests);

        ReceivedRequests = receivedRequests;
        SentRequests = sentRequests;
        ListedAt = listedAt;
    }

    public IReadOnlyList<FriendRequestSummary> ReceivedRequests { get; }

    public IReadOnlyList<FriendRequestSummary> SentRequests { get; }

    public DateTime ListedAt { get; }
}
