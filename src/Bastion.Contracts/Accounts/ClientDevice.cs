using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public sealed class ClientDevice
{
    [DataMember]
    public string DeviceFingerprint { get; set; } = string.Empty;

    [DataMember]
    public string ClientVersion { get; set; } = string.Empty;
}
