using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public interface IFriendNotifier
{
    void NotifyPresenceChanged(IReadOnlyList<int> recipientAccountIds, FriendPresenceChange change);
}
