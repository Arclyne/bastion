using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public enum WallOrientation
{
    [EnumMember]
    Horizontal,

    [EnumMember]
    Vertical,
}
