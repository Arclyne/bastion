using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class WallPlacement
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public short MoveNumber { get; set; }

    [DataMember]
    public string? Groove { get; set; }

    [DataMember]
    public WallOrientation Orientation { get; set; }
}
