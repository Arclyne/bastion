using System;
using System.Runtime.Serialization;
using Bastion.Contracts.Friends;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class FriendPresenceNotification : Notification
{
    [DataMember]
    public string? FriendNickname { get; set; }

    [DataMember]
    public string? FriendCode { get; set; }

    [DataMember]
    public PresenceState Presence { get; set; }

    [DataMember]
    public DateTime? LastSeenAt { get; set; }
}
