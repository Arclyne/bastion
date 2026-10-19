using Bastion.Domain.Auditing;

namespace Bastion.Server.Application.Accounts;

public interface ILoginService
{
    LoginOutcome LogIn(AccessAttempt attempt, string password);
}
