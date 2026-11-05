using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Chat;

[ServiceContract]
public interface IChatService
{
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    ChatMessageReceipt SendChatMessage(string? sessionToken, string? text);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    LobbyChatLog OpenLobbyChat(string? sessionToken);
}
