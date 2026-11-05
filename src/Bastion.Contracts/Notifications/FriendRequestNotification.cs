using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class FriendRequestNotification : Notification
{
    [DataMember]
    public string? RequesterNickname { get; set; }

    [DataMember]
    public string? RequesterFriendCode { get; set; }

    [DataMember]
    public DateTime ExpiresAt { get; set; }
}
