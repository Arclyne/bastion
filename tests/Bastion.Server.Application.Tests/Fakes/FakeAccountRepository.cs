using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Domain.Common;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts;
    private readonly List<PlayerIdentity> _playerIdentities;

    public FakeAccountRepository()
    {
        _accounts = [];
        _playerIdentities = [];
    }

    public void AddAccount(Account account)
    {
        _accounts.Add(account);
    }

    public void AddPlayerIdentity(PlayerIdentity playerIdentity)
    {
        _playerIdentities.Add(playerIdentity);
    }

    public Lookup<Account> FindByLoginIdentifier(string loginIdentifier)
    {
        foreach (Account account in _accounts)
        {
            bool isNickname = string.Equals(account.Nickname, loginIdentifier, StringComparison.OrdinalIgnoreCase);
            bool isEmail = string.Equals(account.Email, loginIdentifier, StringComparison.OrdinalIgnoreCase);
            if (isNickname || isEmail)
            {
                return Lookup.Found(account);
            }
        }

        return Lookup.Missing<Account>("Search=LoginIdentifier");
    }

    public Lookup<Account> FindById(int accountId)
    {
        Account? account = _accounts.Find(candidate => candidate.AccountId == accountId);
        return Lookup.FromNullable(account, $"AccountId={accountId}");
    }

    public Lookup<PlayerIdentity> FindPlayerIdentity(int accountId)
    {
        PlayerIdentity? identity = _playerIdentities.Find(playerIdentity => playerIdentity.AccountId == accountId);
        return Lookup.FromNullable(identity, $"AccountId={accountId}");
    }

    public Lookup<PlayerIdentity> FindPlayerIdentityByFriendCode(string friendCode)
    {
        PlayerIdentity? identity = _playerIdentities.Find(playerIdentity => playerIdentity.FriendCode == friendCode);
        return Lookup.FromNullable(identity, $"FriendCode={friendCode}");
    }

    public IReadOnlyList<PlayerIdentity> SearchPlayerIdentities(string nicknamePrefix, int limit)
    {
        IEnumerable<PlayerIdentity> matchingIdentities = _playerIdentities
            .Where(playerIdentity => HasNicknamePrefix(playerIdentity, nicknamePrefix))
            .OrderBy(playerIdentity => playerIdentity.Nickname, StringComparer.OrdinalIgnoreCase)
            .Take(limit);
        return [.. matchingIdentities];
    }

    public IReadOnlyList<PlayerIdentity> ListPlayerIdentities(IReadOnlyCollection<int> accountIds)
    {
        IEnumerable<PlayerIdentity> listedIdentities = _playerIdentities
            .Where(playerIdentity => accountIds.Contains(playerIdentity.AccountId));
        return [.. listedIdentities];
    }

    private static bool HasNicknamePrefix(PlayerIdentity playerIdentity, string nicknamePrefix)
    {
        return playerIdentity.Nickname.StartsWith(nicknamePrefix, StringComparison.OrdinalIgnoreCase);
    }
}
