using System;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public sealed class TestFailedCodeAttemptHistory
{
    private const int SecondsBetweenAttempts = 10;

    private static readonly DateTime _firstAttemptAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsLimitReached_AttemptsBelowLimit_ReturnsFalse()
    {
        int attemptsBelowLimit = RoomPolicy.MaximumFailedCodeAttempts - 1;
        FailedCodeAttemptHistory attempts = RecordAttempts(attemptsBelowLimit);

        bool isLimitReached = attempts.IsLimitReached(_firstAttemptAt);

        Assert.False(isLimitReached);
    }

    [Fact]
    public void IsLimitReached_AttemptsAtLimit_ReturnsTrue()
    {
        FailedCodeAttemptHistory attempts = RecordAttempts(RoomPolicy.MaximumFailedCodeAttempts);

        bool isLimitReached = attempts.IsLimitReached(_firstAttemptAt);

        Assert.True(isLimitReached);
    }

    [Fact]
    public void IsLimitReached_WindowPassedSinceFirstAttempt_ReturnsFalse()
    {
        FailedCodeAttemptHistory attempts = RecordAttempts(RoomPolicy.MaximumFailedCodeAttempts);
        DateTime afterFirstExpires = _firstAttemptAt + RoomPolicy.FailedCodeWindow;

        bool isLimitReached = attempts.IsLimitReached(afterFirstExpires);

        Assert.False(isLimitReached);
    }

    [Fact]
    public void GetRetryDelay_LimitReached_WaitsUntilFirstAttemptExpires()
    {
        FailedCodeAttemptHistory attempts = RecordAttempts(RoomPolicy.MaximumFailedCodeAttempts);
        var elapsed = TimeSpan.FromSeconds(SecondsBetweenAttempts);
        DateTime now = _firstAttemptAt + elapsed;
        TimeSpan expectedDelay = RoomPolicy.FailedCodeWindow - elapsed;

        TimeSpan retryDelay = attempts.GetRetryDelay(now);

        Assert.Equal(expectedDelay, retryDelay);
    }

    private static FailedCodeAttemptHistory RecordAttempts(int attemptCount)
    {
        var attempts = new FailedCodeAttemptHistory();
        for (int attemptIndex = 0; attemptIndex < attemptCount; attemptIndex++)
        {
            attempts.Record(_firstAttemptAt);
        }

        return attempts;
    }
}
