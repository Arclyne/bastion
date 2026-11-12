using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class FriendRequestList
{
    [DataMember]
    public FriendRequestEntry[]? ReceivedRequests { get; set; }

    [DataMember]
    public FriendRequestEntry[]? SentRequests { get; set; }

    [DataMember]
    public DateTime ListedAt { get; set; }
}
