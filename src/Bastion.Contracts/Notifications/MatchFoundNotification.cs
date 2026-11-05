using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchFoundNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public MatchFoundParticipant[]? Participants { get; set; }
}
