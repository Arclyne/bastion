using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class FriendshipStartedNotification : Notification
{
    [DataMember]
    public string? FriendNickname { get; set; }

    [DataMember]
    public string? FriendCode { get; set; }
}
