using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendshipRepository : IFriendshipRepository
{
    private readonly Dictionary<int, List<FriendSummary>> _friendsByAccountId;

    public FakeFriendshipRepository()
    {
        _friendsByAccountId = [];
        AddedFriendships = [];
        RemovedFriendAccountIds = [];
    }

    public List<Friendship> AddedFriendships { get; }

    public List<int> RemovedFriendAccountIds { get; }

    public void AddFriendSummary(int accountId, FriendSummary friend)
    {
        List<FriendSummary>? friends = _friendsByAccountId.GetValueOrDefault(accountId);
        if (friends is null)
        {
            friends = [];
            _friendsByAccountId[accountId] = friends;
        }

        friends.Add(friend);
    }

    public IReadOnlyList<FriendSummary> ListFriends(int accountId)
    {
        IReadOnlyList<FriendSummary>? friends = _friendsByAccountId.GetValueOrDefault(accountId);
        return friends ?? Array.Empty<FriendSummary>();
    }

    public IReadOnlyList<int> ListFriendAccountIds(int accountId)
    {
        IReadOnlyList<FriendSummary> friends = ListFriends(accountId);
        IEnumerable<int> friendAccountIds = friends.Select(friend => friend.Friend.AccountId);
        return new List<int>(friendAccountIds);
    }

    public void Add(Friendship friendship)
    {
        AddedFriendships.Add(friendship);
        FriendSummary largerFriend = CreateSummary(friendship.LargerAccountId);
        FriendSummary smallerFriend = CreateSummary(friendship.SmallerAccountId);
        AddFriendSummary(friendship.SmallerAccountId, largerFriend);
        AddFriendSummary(friendship.LargerAccountId, smallerFriend);
    }

    public bool Remove(int firstAccountId, int secondAccountId)
    {
        bool isFirstRemoved = RemoveSummary(firstAccountId, secondAccountId);
        bool isSecondRemoved = RemoveSummary(secondAccountId, firstAccountId);
        bool isRemoved = isFirstRemoved || isSecondRemoved;
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
        List<FriendSummary>? friends = _friendsByAccountId.GetValueOrDefault(accountId);
        if (friends is null)
        {
            return false;
        }

        int removedCount = friends.RemoveAll(friend => friend.Friend.AccountId == friendAccountId);
        return removedCount > 0;
    }
}
