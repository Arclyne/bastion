using System;
using Bastion.Domain.Accounts;
using Xunit;

namespace Bastion.Domain.Tests.Accounts;

public sealed class TestLockoutPolicy
{
    private const int FailedLoginCountResetHours = 24;

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
        var expectedDuration = TimeSpan.FromMinutes(expectedMinutes);

        TimeSpan duration = LockoutPolicy.GetLockDuration(failures);

        Assert.Equal(expectedDuration, duration);
    }

    [Fact]
    public void FailedLoginCountResetPeriod_Always_IsTwentyFourHours()
    {
        var expectedPeriod = TimeSpan.FromHours(FailedLoginCountResetHours);

        TimeSpan period = LockoutPolicy.FailedLoginCountResetPeriod;

        Assert.Equal(expectedPeriod, period);
    }
}
