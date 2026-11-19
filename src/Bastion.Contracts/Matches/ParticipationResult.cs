using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public enum ParticipationResult
{
    [EnumMember]
    Won,

    [EnumMember]
    Lost,

    [EnumMember]
    Draw,
}
