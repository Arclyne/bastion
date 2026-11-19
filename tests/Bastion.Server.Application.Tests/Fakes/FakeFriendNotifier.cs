using System.Collections.Generic;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendNotifier : IFriendNotifier
{
    public FakeFriendNotifier()
    {
        PresenceRecipientAccountIds = [];
        PresenceChanges = [];
        RequestRecipientAccountIds = [];
        FriendshipStartedAccountIds = [];
    }

    public List<int> PresenceRecipientAccountIds { get; }

    public List<FriendPresenceChange> PresenceChanges { get; }

    public List<int> RequestRecipientAccountIds { get; }

    public List<int> FriendshipStartedAccountIds { get; }

    public void NotifyPresenceChanged(IReadOnlyList<int> recipientAccountIds, FriendPresenceChange change)
    {
        PresenceRecipientAccountIds.AddRange(recipientAccountIds);
        PresenceChanges.Add(change);
    }

    public void NotifyFriendRequestReceived(int recipientAccountId, FriendRequestSummary request)
    {
        RequestRecipientAccountIds.Add(recipientAccountId);
    }

    public void NotifyFriendshipStarted(int recipientAccountId, PlayerIdentity friend)
    {
        FriendshipStartedAccountIds.Add(recipientAccountId);
    }
}
