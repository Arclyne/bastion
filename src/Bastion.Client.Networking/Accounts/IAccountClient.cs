namespace Bastion.Client.Networking.Accounts;

public interface IAccountClient
{
    LoggedInSession LogIn(LoginCredentials credentials);
}
