using System.ServiceModel;

namespace Bastion.Contracts.Accounts;

[ServiceContract]
public interface IAccountService
{
    [OperationContract]
    [FaultContract(typeof(InvalidCredentialsFault))]
    [FaultContract(typeof(AccountRestrictedFault))]
    PlayerSession LogIn(PlayerCredentials credentials, ClientDevice device);
}
