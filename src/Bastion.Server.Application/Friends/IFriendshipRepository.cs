using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public interface IFriendshipRepository
{
    IReadOnlyList<FriendSummary> ListFriends(int accountId);

    IReadOnlyList<int> ListFriendAccountIds(int accountId);
}
