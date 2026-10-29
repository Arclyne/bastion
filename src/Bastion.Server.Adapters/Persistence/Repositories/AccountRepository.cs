using System;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;

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
}
