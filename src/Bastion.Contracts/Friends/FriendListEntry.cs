using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class FriendListEntry
{
    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public string? FriendCode { get; set; }

    [DataMember]
    public PresenceState Presence { get; set; }

    [DataMember]
    public DateTime? LastSeenAt { get; set; }
}
