using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public enum RestrictionReason
{
    [EnumMember]
    Locked,

    [EnumMember]
    Pending,

    [EnumMember]
    Suspended,

    [EnumMember]
    Banned,
}
