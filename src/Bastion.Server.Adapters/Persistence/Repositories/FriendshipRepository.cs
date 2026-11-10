using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class FriendshipRepository : IFriendshipRepository
{
    private readonly BastionDbContext _context;

    public FriendshipRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public IReadOnlyList<FriendSummary> ListFriends(int accountId)
    {
        IQueryable<int> friendAccountIds = SelectFriendAccountIds(accountId);
        IQueryable<Account> friendAccounts = _context.Accounts
            .AsNoTracking()
            .Where(account => friendAccountIds.Contains(account.AccountId))
            .Where(account => account.AccountStatus != AccountStatus.Deleted);
        var friends = new List<Account>(friendAccounts);
        IEnumerable<FriendSummary> summaries = friends
            .Select(ToFriendSummary);
        return new List<FriendSummary>(summaries);
    }

    public IReadOnlyList<int> ListFriendAccountIds(int accountId)
    {
        IQueryable<int> friendAccountIds = SelectFriendAccountIds(accountId);
        return new List<int>(friendAccountIds);
    }

    public void Add(Friendship friendship)
    {
        ArgumentNullException.ThrowIfNull(friendship);

        _context.Friendships.Add(friendship);
    }

    public bool Remove(int firstAccountId, int secondAccountId)
    {
        int smallerAccountId = Math.Min(firstAccountId, secondAccountId);
        int largerAccountId = Math.Max(firstAccountId, secondAccountId);
        DeletedRowCount deleted = _context.Database
            .SqlQuery<DeletedRowCount>(
                $"EXEC dbo.usp_Friendship_Delete @AccountAId = {smallerAccountId}, @AccountBId = {largerAccountId}")
            .AsEnumerable()
            .Single();
        return deleted.DeletedRows > 0;
    }

    private static FriendSummary ToFriendSummary(Account friend)
    {
        var identity = PlayerIdentity.FromAccount(friend);
        return new FriendSummary(identity, friend.LastSeenAt);
    }

    private IQueryable<int> SelectFriendAccountIds(int accountId)
    {
        IQueryable<int> friendIdsStoredAsLarger = _context.Friendships
            .Where(friendship => friendship.SmallerAccountId == accountId)
            .Select(friendship => friendship.LargerAccountId);
        IQueryable<int> friendIdsStoredAsSmaller = _context.Friendships
            .Where(friendship => friendship.LargerAccountId == accountId)
            .Select(friendship => friendship.SmallerAccountId);
        return friendIdsStoredAsLarger.Concat(friendIdsStoredAsSmaller);
    }
}
