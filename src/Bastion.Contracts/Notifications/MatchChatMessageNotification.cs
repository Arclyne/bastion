using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchChatMessageNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public long MessageId { get; set; }

    [DataMember]
    public string? SenderNickname { get; set; }

    [DataMember]
    public string? Text { get; set; }
}
