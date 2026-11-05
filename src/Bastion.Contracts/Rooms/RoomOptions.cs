using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public sealed class RoomOptions
{
    [DataMember]
    public RoomGameMode GameMode { get; set; }

    [DataMember]
    public byte WallsPerPlayer { get; set; }

    [DataMember]
    public byte ClockMinutes { get; set; }
}
