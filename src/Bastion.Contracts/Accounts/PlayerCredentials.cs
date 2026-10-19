using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class PlayerCredentials
{
    [DataMember]
    public string Identifier { get; set; } = string.Empty;

    [DataMember]
    public string Password { get; set; } = string.Empty;
}
