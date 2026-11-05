using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class FriendRequestList
{
    [DataMember]
    public FriendRequestEntry[]? Received { get; set; }

    [DataMember]
    public FriendRequestEntry[]? Sent { get; set; }

    [DataMember]
    public DateTime ListedAt { get; set; }
}
