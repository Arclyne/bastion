using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendshipRepository : IFriendshipRepository
{
    private readonly Dictionary<int, List<FriendSummary>> _friendsByAccountId =
        new Dictionary<int, List<FriendSummary>>();

    public void Store(int accountId, FriendSummary friend)
    {
        ArgumentNullException.ThrowIfNull(friend);

        if (!_friendsByAccountId.TryGetValue(accountId, out List<FriendSummary>? friends))
        {
            friends = new List<FriendSummary>();
            _friendsByAccountId[accountId] = friends;
        }

        friends.Add(friend);
    }

    public IReadOnlyList<FriendSummary> ListFriends(int accountId)
    {
        if (!_friendsByAccountId.TryGetValue(accountId, out List<FriendSummary>? friends))
        {
            return Array.Empty<FriendSummary>();
        }

        return friends;
    }

    public IReadOnlyList<int> ListFriendAccountIds(int accountId)
    {
        return ListFriends(accountId).Select(friend => friend.Friend.AccountId).ToList();
    }
}
