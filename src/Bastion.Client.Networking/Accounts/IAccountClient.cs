namespace Bastion.Client.Networking.Accounts;

public interface IAccountClient
{
    SignedInSession LogIn(LoginCredentials credentials);
}
