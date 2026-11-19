using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class OpponentReconnectedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public byte TurnOrder { get; set; }
}
