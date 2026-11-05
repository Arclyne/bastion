using System;

namespace Bastion.Domain.Moderation;

public class Sanction
{
    public Sanction(int accountId, SanctionScope scope, DateTime? endedAt)
    {
        AccountId = accountId;
        Scope = scope;
        EndedAt = endedAt;
    }

    private Sanction()
    {
    }

    public int SanctionId { get; private set; }

    public int AccountId { get; private set; }

    public SanctionScope Scope { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public bool IsPermanent => EndedAt is null;

    public void Revoke(DateTime revokedAt)
    {
        RevokedAt = revokedAt;
    }

    public bool IsInEffectAt(DateTime now)
    {
        return RevokedAt is null && (IsPermanent || EndedAt > now);
    }
}
