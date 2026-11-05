using System.ServiceModel;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.PrivateChat;

[ServiceContract]
public interface IConversationService
{
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    ConversationEntry[] ListConversations(string? sessionToken);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    PrivateConversationLog OpenConversation(string? sessionToken, string? friendCode);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    ChatMessageReceipt SendPrivateMessage(string? sessionToken, string? friendCode, string? text);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    void MarkConversationRead(string? sessionToken, string? friendCode);
}
