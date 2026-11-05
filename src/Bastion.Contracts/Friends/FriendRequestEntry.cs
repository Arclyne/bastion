using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class FriendRequestEntry
{
    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public string? FriendCode { get; set; }

    [DataMember]
    public DateTime RequestedAt { get; set; }

    [DataMember]
    public DateTime ExpiresAt { get; set; }
}
