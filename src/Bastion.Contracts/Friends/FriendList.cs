using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class FriendList
{
    [DataMember]
    public string? OwnFriendCode { get; set; }

    [DataMember]
    public FriendListEntry[]? Friends { get; set; }

    [DataMember]
    public DateTime ListedAt { get; set; }
}
