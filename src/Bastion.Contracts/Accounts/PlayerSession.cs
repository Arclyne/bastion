using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class PlayerSession
{
    [DataMember]
    public string Token { get; set; } = string.Empty;

    [DataMember]
    public string Nickname { get; set; } = string.Empty;

    [DataMember]
    public string PreferredLanguage { get; set; } = string.Empty;
}
