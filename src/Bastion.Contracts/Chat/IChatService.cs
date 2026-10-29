using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Chat;

[ServiceContract]
public interface IChatService
{
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(InvalidChatMessageFault))]
    void SendChatMessage(string sessionToken, string text);
}
