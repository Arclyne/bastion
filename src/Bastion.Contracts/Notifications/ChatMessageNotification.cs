using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class ChatMessageNotification : Notification
{
    [DataMember]
    public string SenderNickname { get; set; } = string.Empty;

    [DataMember]
    public string Text { get; set; } = string.Empty;
}
