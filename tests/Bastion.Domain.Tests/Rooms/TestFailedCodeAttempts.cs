using System;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public class TestFailedCodeAttempts
{
    private const int SecondsBetweenAttempts = 10;

    private static readonly DateTime _firstAttemptAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsLimitReached_AttemptsBelowLimit_ReturnsFalse()
    {
        FailedCodeAttempts attempts = RecordAttempts(RoomRules.MaximumFailedCodeAttempts - 1);

        bool isLimitReached = attempts.IsLimitReached(_firstAttemptAt);

        Assert.False(isLimitReached);
    }

    [Fact]
    public void IsLimitReached_AttemptsAtLimit_ReturnsTrue()
    {
        FailedCodeAttempts attempts = RecordAttempts(RoomRules.MaximumFailedCodeAttempts);

        bool isLimitReached = attempts.IsLimitReached(_firstAttemptAt);

        Assert.True(isLimitReached);
    }

    [Fact]
    public void IsLimitReached_WindowPassedSinceFirstAttempt_ReturnsFalse()
    {
        FailedCodeAttempts attempts = RecordAttempts(RoomRules.MaximumFailedCodeAttempts);
        DateTime afterFirstExpires = _firstAttemptAt + RoomRules.FailedCodeWindow;

        bool isLimitReached = attempts.IsLimitReached(afterFirstExpires);

        Assert.False(isLimitReached);
    }

    [Fact]
    public void GetRetryDelay_LimitReached_WaitsUntilFirstAttemptExpires()
    {
        FailedCodeAttempts attempts = RecordAttempts(RoomRules.MaximumFailedCodeAttempts);
        DateTime now = _firstAttemptAt.AddSeconds(SecondsBetweenAttempts);

        TimeSpan retryDelay = attempts.GetRetryDelay(now);

        Assert.Equal(RoomRules.FailedCodeWindow - TimeSpan.FromSeconds(SecondsBetweenAttempts), retryDelay);
    }

    private static FailedCodeAttempts RecordAttempts(int attemptCount)
    {
        var attempts = new FailedCodeAttempts();
        for (int attemptIndex = 0; attemptIndex < attemptCount; attemptIndex++)
        {
            attempts.Record(_firstAttemptAt);
        }

        return attempts;
    }
}
