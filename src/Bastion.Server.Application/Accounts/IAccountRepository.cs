using Bastion.Domain.Accounts;

namespace Bastion.Server.Application.Accounts;

public interface IAccountRepository
{
    Account? FindByIdentifier(string identifier);

    Account? FindById(int accountId);
}
