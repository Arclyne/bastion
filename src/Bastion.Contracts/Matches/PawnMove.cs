using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class PawnMove
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public short MoveNumber { get; set; }

    [DataMember]
    public string? ToSquare { get; set; }
}
