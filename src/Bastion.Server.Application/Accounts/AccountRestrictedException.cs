using System;

namespace Bastion.Server.Application.Accounts;

public sealed class AccountRestrictedException : Exception
{
    public AccountRestrictedException(AccountRestriction restriction, int lockMinutes)
        : base($"The account cannot sign in. Restriction={restriction}")
    {
        Restriction = restriction;
        LockMinutes = lockMinutes;
    }

    public AccountRestriction Restriction { get; }

    public int LockMinutes { get; }
}
