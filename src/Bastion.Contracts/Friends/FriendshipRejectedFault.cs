using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class FriendshipRejectedFault
{
    [DataMember]
    public FriendshipRejectionReason Reason { get; set; }
}
