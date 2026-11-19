using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class DrawOfferExpiredNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }
}
