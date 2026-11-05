using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Friends;

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
        IQueryable<int> friendAccountIds = _context.Friendships
            .Where(friendship => friendship.AccountAId == accountId)
            .Select(friendship => friendship.AccountBId)
            .Concat(_context.Friendships
                .Where(friendship => friendship.AccountBId == accountId)
                .Select(friendship => friendship.AccountAId));

        return _context.Accounts
            .Where(account => friendAccountIds.Contains(account.AccountId)
                && account.AccountStatus != AccountStatus.Deleted)
            .Select(account => new FriendSummary(
                new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode),
                account.LastSeenAt))
            .ToList();
    }
}
