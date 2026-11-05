using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public enum FriendRequestResult
{
    [EnumMember]
    RequestSent,

    [EnumMember]
    FriendshipStarted,
}
