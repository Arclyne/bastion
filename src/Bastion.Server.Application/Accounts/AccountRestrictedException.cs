using System;

namespace Bastion.Server.Application.Accounts;

public sealed class AccountRestrictedException : Exception
{
    private const int NoLockMinutes = 0;

    public AccountRestrictedException(AccountRestriction restriction)
        : this(restriction, NoLockMinutes)
    {
    }

    public AccountRestrictedException(AccountRestriction restriction, int lockMinutes)
        : base($"The account cannot log in. Restriction={restriction}")
    {
        Restriction = restriction;
        LockMinutes = lockMinutes;
    }

    public AccountRestriction Restriction { get; }

    public int LockMinutes { get; }
}
