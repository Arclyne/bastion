using System;

namespace Bastion.Domain.Accounts;

public static class LockoutPolicy
{
    private const int FirstThreshold = 3;
    private const int SecondThreshold = 6;
    private const int ThirdThreshold = 9;
    private const int FirstLockMinutes = 5;
    private const int SecondLockMinutes = 30;
    private const int ThirdLockMinutes = 60;
    private const int CounterResetHours = 24;

    public static TimeSpan CounterResetPeriod { get; } = TimeSpan.FromHours(CounterResetHours);

    public static TimeSpan GetLockDuration(int consecutiveFailures)
    {
        if (consecutiveFailures >= ThirdThreshold)
        {
            return TimeSpan.FromMinutes(ThirdLockMinutes);
        }

        if (consecutiveFailures == SecondThreshold)
        {
            return TimeSpan.FromMinutes(SecondLockMinutes);
        }

        return consecutiveFailures == FirstThreshold ? TimeSpan.FromMinutes(FirstLockMinutes) : TimeSpan.Zero;
    }
}
