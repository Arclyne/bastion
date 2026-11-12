using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = new List<Account>();
    private readonly List<PlayerIdentity> _playerIdentities = new List<PlayerIdentity>();

    public void AddAccount(Account account)
    {
        _accounts.Add(account);
    }

    public void AddPlayerIdentity(PlayerIdentity playerIdentity)
    {
        _playerIdentities.Add(playerIdentity);
    }

    public Account? FindByLoginIdentifier(string loginIdentifier)
    {
        foreach (Account account in _accounts)
        {
            bool isNickname = string.Equals(account.Nickname, loginIdentifier, StringComparison.OrdinalIgnoreCase);
            bool isEmail = string.Equals(account.Email, loginIdentifier, StringComparison.OrdinalIgnoreCase);
            if (isNickname || isEmail)
            {
                return account;
            }
        }

        return null;
    }

    public Account? FindById(int accountId)
    {
        return _accounts.Find(account => account.AccountId == accountId);
    }

    public PlayerIdentity? FindPlayerIdentity(int accountId)
    {
        return _playerIdentities.Find(playerIdentity => playerIdentity.AccountId == accountId);
    }

    public PlayerIdentity? FindPlayerIdentityByFriendCode(string friendCode)
    {
        return _playerIdentities.Find(playerIdentity => playerIdentity.FriendCode == friendCode);
    }

    public IReadOnlyList<PlayerIdentity> SearchPlayerIdentities(string nicknamePrefix, int limit)
    {
        IEnumerable<PlayerIdentity> matchingIdentities = _playerIdentities
            .Where(playerIdentity =>
                playerIdentity.Nickname.StartsWith(nicknamePrefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(playerIdentity => playerIdentity.Nickname, StringComparer.OrdinalIgnoreCase)
            .Take(limit);
        return new List<PlayerIdentity>(matchingIdentities);
    }

    public IReadOnlyList<PlayerIdentity> ListPlayerIdentities(IReadOnlyCollection<int> accountIds)
    {
        IEnumerable<PlayerIdentity> listedIdentities = _playerIdentities
            .Where(playerIdentity => accountIds.Contains(playerIdentity.AccountId));
        return new List<PlayerIdentity>(listedIdentities);
    }
}
