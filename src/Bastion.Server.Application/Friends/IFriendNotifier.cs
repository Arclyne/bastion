using System.Collections.Generic;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public interface IFriendNotifier
{
    void NotifyPresenceChanged(IReadOnlyList<int> recipientAccountIds, FriendPresenceChange change);

    void NotifyFriendRequestReceived(int recipientAccountId, FriendRequestSummary request);

    void NotifyFriendshipStarted(int accountId, PlayerIdentity friend);
}
