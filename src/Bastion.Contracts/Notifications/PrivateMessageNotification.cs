using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class PrivateMessageNotification : Notification
{
    [DataMember]
    public long MessageId { get; set; }

    [DataMember]
    public string? SenderNickname { get; set; }

    [DataMember]
    public string? SenderFriendCode { get; set; }

    [DataMember]
    public string? Text { get; set; }
}
