using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public sealed class RoomMemberState
{
    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public int SeatIndex { get; set; }

    [DataMember]
    public bool IsReady { get; set; }

    [DataMember]
    public bool IsHost { get; set; }
}
