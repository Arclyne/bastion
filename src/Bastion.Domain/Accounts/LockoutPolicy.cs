using System;

namespace Bastion.Domain.Accounts;

public static class LockoutPolicy
{
    private const int FirstLockThreshold = 3;
    private const int SecondLockThreshold = 6;
    private const int ThirdLockThreshold = 9;
    private const int FirstLockMinutes = 5;
    private const int SecondLockMinutes = 30;
    private const int ThirdLockMinutes = 60;
    private const int CounterResetHours = 24;

    public static TimeSpan FailedLoginCountResetPeriod { get; } = TimeSpan.FromHours(CounterResetHours);

    public static TimeSpan GetLockDuration(int failedLoginCount)
    {
        if (failedLoginCount >= ThirdLockThreshold)
        {
            return TimeSpan.FromMinutes(ThirdLockMinutes);
        }

        if (failedLoginCount == SecondLockThreshold)
        {
            return TimeSpan.FromMinutes(SecondLockMinutes);
        }

        if (failedLoginCount == FirstLockThreshold)
        {
            return TimeSpan.FromMinutes(FirstLockMinutes);
        }

        return TimeSpan.Zero;
    }
}
