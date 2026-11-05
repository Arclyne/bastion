using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Friends;

[ServiceContract]
public interface IFriendshipService
{
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    FriendList ListFriends(string? sessionToken);
}
