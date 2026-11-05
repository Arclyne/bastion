using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class RoomClosedNotification : Notification
{
    [DataMember]
    public string? Code { get; set; }

    [DataMember]
    public RoomClosedReason Reason { get; set; }
}
