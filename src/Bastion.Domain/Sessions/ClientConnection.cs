namespace Bastion.Domain.Sessions;

public sealed record ClientConnection(string IpAddress, string DeviceFingerprint, string ClientVersion);
