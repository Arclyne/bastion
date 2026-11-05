using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public sealed class RoomRejectedFault
{
    [DataMember]
    public RoomRejectionReason Reason { get; set; }

    [DataMember]
    public int RetryAfterSeconds { get; set; }
}
