using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly BastionDbContext _context;

    public AccountRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public Account? FindByLoginIdentifier(string loginIdentifier)
    {
        ArgumentNullException.ThrowIfNull(loginIdentifier);

        string email = loginIdentifier.ToLowerInvariant();
        return _context.Accounts.FirstOrDefault(
            account => account.Nickname == loginIdentifier || account.Email == email);
    }

    public Account? FindById(int accountId)
    {
        return _context.Accounts.FirstOrDefault(account => account.AccountId == accountId);
    }

    public PlayerIdentity? FindPlayerIdentity(int accountId)
    {
        IQueryable<Account> matchingAccounts = _context.Accounts
            .AsNoTracking()
            .Where(account => account.AccountId == accountId);
        return FindIdentity(matchingAccounts);
    }

    public IReadOnlyList<PlayerIdentity> ListPlayerIdentities(IReadOnlyCollection<int> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        IQueryable<Account> requestedAccounts = _context.Accounts
            .AsNoTracking()
            .Where(account => accountIds.Contains(account.AccountId));
        return ListIdentities(requestedAccounts);
    }

    public PlayerIdentity? FindPlayerIdentityByFriendCode(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        IQueryable<Account> playersWithCode = SelectActivePlayers()
            .Where(account => account.FriendCode == friendCode);
        return FindIdentity(playersWithCode);
    }

    public IReadOnlyList<PlayerIdentity> SearchPlayerIdentities(string nicknamePrefix, int limit)
    {
        ArgumentNullException.ThrowIfNull(nicknamePrefix);

        IQueryable<Account> matchingPlayers = SelectActivePlayers()
            .Where(account => account.Nickname.StartsWith(nicknamePrefix))
            .OrderBy(account => account.Nickname)
            .Take(limit);
        return ListIdentities(matchingPlayers);
    }

    private static PlayerIdentity? FindIdentity(IQueryable<Account> accounts)
    {
        Account? account = accounts.FirstOrDefault();
        if (account is null)
        {
            return null;
        }

        return PlayerIdentity.FromAccount(account);
    }

    private static List<PlayerIdentity> ListIdentities(IQueryable<Account> accounts)
    {
        var loadedAccounts = new List<Account>(accounts);
        IEnumerable<PlayerIdentity> identities = loadedAccounts
            .Select(PlayerIdentity.FromAccount);
        return new List<PlayerIdentity>(identities);
    }

    private IQueryable<Account> SelectActivePlayers()
    {
        IQueryable<Account> activePlayers = _context.Accounts
            .AsNoTracking()
            .Where(account => account.AccountType == AccountType.Registered)
            .Where(account => account.AccountStatus == AccountStatus.Active);
        return activePlayers;
    }
}
