using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public enum WallRejectionReason
{
    [EnumMember]
    NoWallsLeft,

    [EnumMember]
    OutOfBoard,

    [EnumMember]
    GrooveTaken,

    [EnumMember]
    WouldBlockPlayer,
}
