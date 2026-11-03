using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class PlayerSession
{
    [DataMember]
    public string? Token { get; set; }

    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public string? PreferredLanguage { get; set; }
}
