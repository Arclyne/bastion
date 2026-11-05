using System;
using System.Collections.Generic;

namespace Bastion.Domain.Rooms;

public sealed class FailedCodeAttempts
{
    private readonly Queue<DateTime> _attemptTimes = new Queue<DateTime>();

    public bool HasAttempts => _attemptTimes.Count > 0;

    public void Record(DateTime attemptedAt)
    {
        ForgetOldAttempts(attemptedAt);
        _attemptTimes.Enqueue(attemptedAt);
    }

    public bool IsLimitReached(DateTime now)
    {
        ForgetOldAttempts(now);
        return _attemptTimes.Count >= RoomRules.MaximumFailedCodeAttempts;
    }

    public TimeSpan GetRetryDelay(DateTime now)
    {
        if (!IsLimitReached(now))
        {
            return TimeSpan.Zero;
        }

        return _attemptTimes.Peek() + RoomRules.FailedCodeWindow - now;
    }

    private void ForgetOldAttempts(DateTime now)
    {
        while (_attemptTimes.Count > 0 && _attemptTimes.Peek() + RoomRules.FailedCodeWindow <= now)
        {
            _attemptTimes.Dequeue();
        }
    }
}
