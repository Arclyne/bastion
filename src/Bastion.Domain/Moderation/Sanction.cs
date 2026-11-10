using System;

namespace Bastion.Domain.Moderation;

public class Sanction
{
    public Sanction(int accountId, SanctionScope scope, DateTime? endsAt)
    {
        AccountId = accountId;
        Scope = scope;
        EndsAt = endsAt;
    }

    private Sanction()
    {
    }

    public int SanctionId { get; private set; }

    public int AccountId { get; private set; }

    public SanctionScope Scope { get; private set; }

    public DateTime? EndsAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public bool IsPermanent()
    {
        return EndsAt is null;
    }

    public bool IsInEffectAt(DateTime now)
    {
        if (RevokedAt is not null)
        {
            return false;
        }

        if (IsPermanent())
        {
            return true;
        }

        return EndsAt > now;
    }
}
