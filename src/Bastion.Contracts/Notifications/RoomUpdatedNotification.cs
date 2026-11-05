using System.Runtime.Serialization;
using Bastion.Contracts.Rooms;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class RoomUpdatedNotification : Notification
{
    [DataMember]
    public RoomState? Room { get; set; }
}
