using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class AccessDecision
{
    [DataMember]
    public AccessDecisionCode Code { get; set; }

    [DataMember]
    public PlayerSession? Session { get; set; }

    [DataMember]
    public int LockMinutes { get; set; }
}
