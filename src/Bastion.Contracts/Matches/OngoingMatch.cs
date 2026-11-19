using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class OngoingMatch
{
    [DataMember]
    public bool IsFound { get; set; }

    [DataMember]
    public MatchSnapshot? Snapshot { get; set; }
}
