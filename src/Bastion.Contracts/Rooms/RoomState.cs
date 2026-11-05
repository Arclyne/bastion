using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public sealed class RoomState
{
    [DataMember]
    public string? Code { get; set; }

    [DataMember]
    public RoomOptions? Options { get; set; }

    [DataMember]
    public int SeatCount { get; set; }

    [DataMember]
    public RoomMemberState[]? Members { get; set; }
}
