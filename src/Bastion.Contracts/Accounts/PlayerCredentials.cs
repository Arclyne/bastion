using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class PlayerCredentials
{
    [DataMember]
    public string? Identifier { get; set; }

    [DataMember]
    public string? Password { get; set; }
}
