using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class DrawRejectedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }
}
