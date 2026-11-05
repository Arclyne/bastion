using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendshipRepository : IFriendshipRepository
{
    private readonly Dictionary<int, List<FriendSummary>> _friendsByAccountId =
        new Dictionary<int, List<FriendSummary>>();

    public List<Friendship> AddedFriendships { get; } = new List<Friendship>();

    public List<int> RemovedFriendAccountIds { get; } = new List<int>();

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

    public void Add(Friendship friendship)
    {
        ArgumentNullException.ThrowIfNull(friendship);

        AddedFriendships.Add(friendship);
        Store(friendship.AccountAId, CreateSummary(friendship.AccountBId));
        Store(friendship.AccountBId, CreateSummary(friendship.AccountAId));
    }

    public bool Remove(int firstAccountId, int secondAccountId)
    {
        bool removedFirst = RemoveSummary(firstAccountId, secondAccountId);
        bool removedSecond = RemoveSummary(secondAccountId, firstAccountId);
        if (removedFirst || removedSecond)
        {
            RemovedFriendAccountIds.Add(secondAccountId);
        }

        return removedFirst || removedSecond;
    }

    private static FriendSummary CreateSummary(int friendAccountId)
    {
        return new FriendSummary(new PlayerIdentity(friendAccountId, string.Empty, string.Empty), null);
    }

    private bool RemoveSummary(int accountId, int friendAccountId)
    {
        if (!_friendsByAccountId.TryGetValue(accountId, out List<FriendSummary>? friends))
        {
            return false;
        }

        return friends.RemoveAll(friend => friend.Friend.AccountId == friendAccountId) > 0;
    }
}
