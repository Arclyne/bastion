using System;
using Bastion.Domain.Accounts;
using Xunit;

namespace Bastion.Domain.Tests.Accounts;

public class TestLockoutPolicy
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 5)]
    [InlineData(4, 0)]
    [InlineData(6, 30)]
    [InlineData(7, 0)]
    [InlineData(9, 60)]
    [InlineData(12, 60)]
    public void GetLockDuration_ConsecutiveFailures_ReturnsEscalatingMinutes(int failures, int expectedMinutes)
    {
        TimeSpan duration = LockoutPolicy.GetLockDuration(failures);

        Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), duration);
    }

    [Fact]
    public void CounterResetPeriod_Always_IsTwentyFourHours()
    {
        TimeSpan period = LockoutPolicy.CounterResetPeriod;

        Assert.Equal(TimeSpan.FromHours(24), period);
    }
}
