using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Domain.Friends;
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

        return _context.Accounts
            .Where(account => friendAccountIds.Contains(account.AccountId)
                && account.AccountStatus != AccountStatus.Deleted)
            .Select(account => new FriendSummary(
                new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode),
                account.LastSeenAt))
            .ToList();
    }

    public IReadOnlyList<int> ListFriendAccountIds(int accountId)
    {
        return SelectFriendAccountIds(accountId).ToList();
    }

    public void Add(Friendship friendship)
    {
        ArgumentNullException.ThrowIfNull(friendship);

        _context.Friendships.Add(friendship);
    }

    public bool Remove(int firstAccountId, int secondAccountId)
    {
        int accountAId = Math.Min(firstAccountId, secondAccountId);
        int accountBId = Math.Max(firstAccountId, secondAccountId);
        DeletedRowCount deleted = _context.Database
            .SqlQuery<DeletedRowCount>(
                $"EXEC dbo.usp_Friendship_Delete @AccountAId = {accountAId}, @AccountBId = {accountBId}")
            .AsEnumerable()
            .Single();
        return deleted.DeletedRows > 0;
    }

    private IQueryable<int> SelectFriendAccountIds(int accountId)
    {
        return _context.Friendships
            .Where(friendship => friendship.AccountAId == accountId)
            .Select(friendship => friendship.AccountBId)
            .Concat(_context.Friendships
                .Where(friendship => friendship.AccountBId == accountId)
                .Select(friendship => friendship.AccountAId));
    }
}
