using System;
using System.Collections.Generic;

namespace Bastion.Domain.Rooms;

public sealed class FailedCodeAttemptHistory
{
    private readonly Queue<DateTime> _attemptTimes = new Queue<DateTime>();

    public bool HasAttempts()
    {
        return _attemptTimes.Count > 0;
    }

    public void Record(DateTime attemptedAt)
    {
        ForgetOldAttempts(attemptedAt);
        _attemptTimes.Enqueue(attemptedAt);
    }

    public bool IsLimitReached(DateTime now)
    {
        ForgetOldAttempts(now);
        return _attemptTimes.Count >= RoomPolicy.MaximumFailedCodeAttempts;
    }

    public TimeSpan GetRetryDelay(DateTime now)
    {
        if (!IsLimitReached(now))
        {
            return TimeSpan.Zero;
        }

        DateTime retryAt = GetOldestAttemptExpiry();
        TimeSpan retryDelay = retryAt - now;
        return retryDelay;
    }

    private void ForgetOldAttempts(DateTime now)
    {
        while (HasAttempts() && GetOldestAttemptExpiry() <= now)
        {
            _attemptTimes.Dequeue();
        }
    }

    private DateTime GetOldestAttemptExpiry()
    {
        DateTime oldestAttempt = _attemptTimes.Peek();
        DateTime oldestAttemptExpiry = oldestAttempt + RoomPolicy.FailedCodeWindow;
        return oldestAttemptExpiry;
    }
}
