using System.ServiceModel;

namespace Bastion.Contracts.Accounts;

[ServiceContract]
public interface IAccountService
{
    [OperationContract]
    AccessDecision LogIn(PlayerCredentials credentials, ClientDevice device);
}
