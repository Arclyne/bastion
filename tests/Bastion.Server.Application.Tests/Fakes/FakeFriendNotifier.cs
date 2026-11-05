using System;
using System.Collections.Generic;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendNotifier : IFriendNotifier
{
    public List<int> RecipientAccountIds { get; } = new List<int>();

    public List<FriendPresenceChange> Changes { get; } = new List<FriendPresenceChange>();

    public void NotifyPresenceChanged(IReadOnlyList<int> recipientAccountIds, FriendPresenceChange change)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(change);

        RecipientAccountIds.AddRange(recipientAccountIds);
        Changes.Add(change);
    }
}
