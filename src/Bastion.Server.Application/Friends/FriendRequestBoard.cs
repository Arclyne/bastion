using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRequestBoard
{
    public FriendRequestBoard(
        IReadOnlyList<FriendRequestSummary> received,
        IReadOnlyList<FriendRequestSummary> sent,
        DateTime listedAt)
    {
        ArgumentNullException.ThrowIfNull(received);
        ArgumentNullException.ThrowIfNull(sent);

        Received = received;
        Sent = sent;
        ListedAt = listedAt;
    }

    public IReadOnlyList<FriendRequestSummary> Received { get; }

    public IReadOnlyList<FriendRequestSummary> Sent { get; }

    public DateTime ListedAt { get; }
}
