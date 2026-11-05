using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly BastionDbContext _context;

    public AccountRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public Account? FindByIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        string email = identifier.ToLowerInvariant();
        return _context.Accounts.FirstOrDefault(
            account => account.Nickname == identifier || account.Email == email);
    }

    public Account? FindById(int accountId)
    {
        return _context.Accounts.FirstOrDefault(account => account.AccountId == accountId);
    }

    public PlayerIdentity? FindPlayerIdentity(int accountId)
    {
        return _context.Accounts
            .Where(account => account.AccountId == accountId)
            .Select(account => new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode))
            .FirstOrDefault();
    }

    public PlayerIdentity? FindPlayerIdentityByFriendCode(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        return SelectActivePlayers()
            .Where(account => account.FriendCode == friendCode)
            .Select(account => new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode))
            .FirstOrDefault();
    }

    public IReadOnlyList<PlayerIdentity> SearchPlayerIdentities(string nicknamePrefix, int limit)
    {
        ArgumentNullException.ThrowIfNull(nicknamePrefix);

        return SelectActivePlayers()
            .Where(account => account.Nickname.StartsWith(nicknamePrefix))
            .OrderBy(account => account.Nickname)
            .Take(limit)
            .Select(account => new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode))
            .ToList();
    }

    private IQueryable<Account> SelectActivePlayers()
    {
        return _context.Accounts.Where(account =>
            account.AccountType == AccountType.Registered && account.AccountStatus == AccountStatus.Active);
    }
}
