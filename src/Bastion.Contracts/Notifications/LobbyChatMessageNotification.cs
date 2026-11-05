using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class LobbyChatMessageNotification : Notification
{
    [DataMember]
    public string? SenderNickname { get; set; }

    [DataMember]
    public string? Text { get; set; }
}
