using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Friends;

[ServiceContract]
public interface IFriendshipService
{
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    FriendList ListFriends(string? sessionToken);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    PlayerSearchEntry[] SearchPlayers(string? sessionToken, string? text);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    FriendRequestResult SendFriendRequest(string? sessionToken, string? friendCode);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    FriendRequestList ListFriendRequests(string? sessionToken);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void AcceptFriendRequest(string? sessionToken, string? requesterFriendCode);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void RejectFriendRequest(string? sessionToken, string? requesterFriendCode);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void CancelFriendRequest(string? sessionToken, string? recipientFriendCode);

    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void RemoveFriend(string? sessionToken, string? friendCode);
}
