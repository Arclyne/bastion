using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchFoundNotification : Notification
{
    [DataMember]
    public string[]? ParticipantNicknames { get; set; }
}
