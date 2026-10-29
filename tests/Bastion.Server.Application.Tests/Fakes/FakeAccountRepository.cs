using System;
using System.Collections.Generic;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = new List<Account>();

    public void Store(Account account)
    {
        _accounts.Add(account);
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
}
