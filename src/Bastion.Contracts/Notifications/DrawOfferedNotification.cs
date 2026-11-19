using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class DrawOfferedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public byte OffererTurnOrder { get; set; }
}
