using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendshipRepository : IFriendshipRepository
{
    private readonly Dictionary<int, List<FriendSummary>> _friendsByAccountId =
        new Dictionary<int, List<FriendSummary>>();

    public List<Friendship> AddedFriendships { get; } = new List<Friendship>();

    public List<int> RemovedFriendAccountIds { get; } = new List<int>();

    public void AddFriendSummary(int accountId, FriendSummary friend)
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
        IReadOnlyList<FriendSummary> friends = ListFriends(accountId);
        IEnumerable<int> friendAccountIds = friends.Select(friend => friend.Friend.AccountId);
        return new List<int>(friendAccountIds);
    }

    public void Add(Friendship friendship)
    {
        ArgumentNullException.ThrowIfNull(friendship);

        AddedFriendships.Add(friendship);
        FriendSummary largerFriend = CreateSummary(friendship.LargerAccountId);
        FriendSummary smallerFriend = CreateSummary(friendship.SmallerAccountId);
        AddFriendSummary(friendship.SmallerAccountId, largerFriend);
        AddFriendSummary(friendship.LargerAccountId, smallerFriend);
    }

    public bool Remove(int firstAccountId, int secondAccountId)
    {
        bool removedFirst = RemoveSummary(firstAccountId, secondAccountId);
        bool removedSecond = RemoveSummary(secondAccountId, firstAccountId);
        bool isRemoved = removedFirst || removedSecond;
        if (isRemoved)
        {
            RemovedFriendAccountIds.Add(secondAccountId);
        }

        return isRemoved;
    }

    private static FriendSummary CreateSummary(int friendAccountId)
    {
        var friend = new PlayerIdentity(friendAccountId, string.Empty, string.Empty);
        return new FriendSummary(friend, lastSeenAt: null);
    }

    private bool RemoveSummary(int accountId, int friendAccountId)
    {
        if (!_friendsByAccountId.TryGetValue(accountId, out List<FriendSummary>? friends))
        {
            return false;
        }

        int removedCount = friends.RemoveAll(friend => friend.Friend.AccountId == friendAccountId);
        return removedCount > 0;
    }
}
