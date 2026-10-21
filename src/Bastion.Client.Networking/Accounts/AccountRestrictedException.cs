using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class AccountRestrictedException : Exception
{
    public AccountRestrictedException(AccountRestriction restriction, int lockMinutes, Exception innerException)
        : base($"The account cannot sign in. Restriction={restriction}", innerException)
    {
        Restriction = restriction;
        LockMinutes = lockMinutes;
    }

    public AccountRestriction Restriction { get; }

    public int LockMinutes { get; }
}
