using System;

namespace Bastion.Domain.Sessions;

public sealed record SessionTicket
{
    public SessionTicket(byte[] tokenHash, DateTime issuedAt)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        TokenHash = tokenHash;
        IssuedAt = issuedAt;
    }

    public byte[] TokenHash { get; }

    public DateTime IssuedAt { get; }
}
