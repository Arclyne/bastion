using System.Collections.Generic;
using Bastion.Domain.Friends;

namespace Bastion.Server.Application.Friends;

public interface IFriendshipRepository
{
    IReadOnlyList<FriendSummary> ListFriends(int accountId);

    IReadOnlyList<int> ListFriendAccountIds(int accountId);

    void Add(Friendship friendship);
}
