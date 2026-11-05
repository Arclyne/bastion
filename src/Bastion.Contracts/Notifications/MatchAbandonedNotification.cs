using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchAbandonedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public string? AbandoningNickname { get; set; }

    [DataMember]
    public bool IsMatchOver { get; set; }
}
