using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class SnapshotMove
{
    [DataMember]
    public short MoveNumber { get; set; }

    [DataMember]
    public byte TurnOrder { get; set; }

    [DataMember]
    public bool IsWall { get; set; }

    [DataMember]
    public string? Groove { get; set; }

    [DataMember]
    public WallOrientation Orientation { get; set; }
}
