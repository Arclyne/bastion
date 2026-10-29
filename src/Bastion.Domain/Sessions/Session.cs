using System;

namespace Bastion.Domain.Sessions;

public class Session
{
    public const int MaximumLifetimeHours = 24;

    public Session(int accountId, SessionTicket ticket, ClientConnection connection)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(connection);

        AccountId = accountId;
        TokenHash = ticket.TokenHash;
        StartedAt = ticket.IssuedAt;
        LastUsedAt = ticket.IssuedAt;
        ExpiresAt = ticket.IssuedAt.AddHours(MaximumLifetimeHours);
        IpAddress = connection.IpAddress;
        DeviceFingerprint = connection.DeviceFingerprint;
        ClientVersion = connection.ClientVersion;
    }

    private Session()
    {
        TokenHash = Array.Empty<byte>();
        IpAddress = string.Empty;
        DeviceFingerprint = string.Empty;
        ClientVersion = string.Empty;
    }

    public int SessionId { get; private set; }

    public int AccountId { get; private set; }

    public byte[] TokenHash { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime LastUsedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public string IpAddress { get; private set; }

    public string DeviceFingerprint { get; private set; }

    public string ClientVersion { get; private set; }

    public bool IsActiveAt(DateTime now)
    {
        return EndedAt is null && now < ExpiresAt;
    }
}
