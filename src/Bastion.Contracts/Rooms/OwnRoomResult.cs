using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public sealed class OwnRoomResult
{
    [DataMember]
    public bool IsInRoom { get; set; }

    [DataMember]
    public RoomState? Room { get; set; }
}
