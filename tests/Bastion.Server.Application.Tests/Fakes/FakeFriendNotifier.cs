using System;
using System.Collections.Generic;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendNotifier : IFriendNotifier
{
    public List<int> RecipientAccountIds { get; } = new List<int>();

    public List<FriendPresenceChange> Changes { get; } = new List<FriendPresenceChange>();

    public List<int> RequestRecipientAccountIds { get; } = new List<int>();

    public List<int> FriendshipStartedAccountIds { get; } = new List<int>();

    public void NotifyPresenceChanged(IReadOnlyList<int> recipientAccountIds, FriendPresenceChange change)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(change);

        RecipientAccountIds.AddRange(recipientAccountIds);
        Changes.Add(change);
    }

    public void NotifyFriendRequestReceived(int recipientAccountId, FriendRequestSummary request)
    {
        ArgumentNullException.ThrowIfNull(request);

        RequestRecipientAccountIds.Add(recipientAccountId);
    }

    public void NotifyFriendshipStarted(int accountId, PlayerIdentity friend)
    {
        ArgumentNullException.ThrowIfNull(friend);

        FriendshipStartedAccountIds.Add(accountId);
    }
}
