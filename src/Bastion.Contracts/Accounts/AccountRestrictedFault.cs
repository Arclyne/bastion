using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class AccountRestrictedFault
{
    [DataMember]
    public RestrictionReason Reason { get; set; }

    [DataMember]
    public int LockMinutes { get; set; }
}
