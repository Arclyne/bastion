using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = new List<Account>();
    private readonly List<PlayerIdentity> _playerIdentities = new List<PlayerIdentity>();

    public void Store(Account account)
    {
        _accounts.Add(account);
    }

    public void StorePlayerIdentity(PlayerIdentity playerIdentity)
    {
        _playerIdentities.Add(playerIdentity);
    }

    public Account? FindByIdentifier(string identifier)
    {
        foreach (Account account in _accounts)
        {
            bool isNickname = string.Equals(account.Nickname, identifier, StringComparison.OrdinalIgnoreCase);
            bool isEmail = string.Equals(account.Email, identifier, StringComparison.OrdinalIgnoreCase);
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
        return _playerIdentities
            .Where(playerIdentity =>
                playerIdentity.Nickname.StartsWith(nicknamePrefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(playerIdentity => playerIdentity.Nickname, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    public IReadOnlyList<PlayerIdentity> ListPlayerIdentities(IReadOnlyCollection<int> accountIds)
    {
        return _playerIdentities.Where(playerIdentity => accountIds.Contains(playerIdentity.AccountId)).ToList();
    }
}
