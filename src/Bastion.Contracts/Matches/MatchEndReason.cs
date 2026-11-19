using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public enum MatchEndReason
{
    [EnumMember]
    Goal,

    [EnumMember]
    Timeout,

    [EnumMember]
    Resignation,

    [EnumMember]
    Draw,

    [EnumMember]
    Abandonment,
}
