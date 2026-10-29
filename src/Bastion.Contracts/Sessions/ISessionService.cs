using System.ServiceModel;
using Bastion.Contracts.Notifications;

namespace Bastion.Contracts.Sessions;

[ServiceContract(CallbackContract = typeof(INotificationCallback))]
public interface ISessionService
{
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    void Subscribe(string sessionToken);
}
