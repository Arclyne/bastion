namespace Bastion.Client.Networking.Accounts;

public interface IAccountClient
{
    LoginOutcome LogIn(LoginCredentials credentials);
}
