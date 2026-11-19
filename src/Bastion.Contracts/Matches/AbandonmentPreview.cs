using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class AbandonmentPreview
{
    [DataMember]
    public bool IsRanked { get; set; }

    [DataMember]
    public short? InitialElo { get; set; }

    [DataMember]
    public short? FinalElo { get; set; }

    [DataMember]
    public byte? Placement { get; set; }

    [DataMember]
    public int ActivePlayerCount { get; set; }
}
