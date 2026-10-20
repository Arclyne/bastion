using Bastion.Domain.Auditing;

namespace Bastion.Server.Application.Accounts;

public interface ILoginService
{
    SignedInPlayer LogIn(AccessAttempt attempt, string password);
}
