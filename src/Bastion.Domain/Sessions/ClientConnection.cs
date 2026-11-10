using System;

namespace Bastion.Domain.Sessions;

public sealed record ClientConnection
{
    public const int MaximumIpAddressLength = 45;

    public ClientConnection(string ipAddress, string deviceFingerprint, string clientVersion)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(deviceFingerprint);
        ArgumentNullException.ThrowIfNull(clientVersion);

        IpAddress = ipAddress;
        DeviceFingerprint = deviceFingerprint;
        ClientVersion = clientVersion;
    }

    public string IpAddress { get; }

    public string DeviceFingerprint { get; }

    public string ClientVersion { get; }
}
