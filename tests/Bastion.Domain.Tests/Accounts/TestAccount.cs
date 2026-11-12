using System;
using Bastion.Domain.Accounts;
using Xunit;

namespace Bastion.Domain.Tests.Accounts;

public sealed class TestAccount
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";
    private const int FailuresBelowLock = 2;
    private const int FailuresThatLock = 3;
    private const int FirstLockMinutes = 5;
    private const int HoursAfterResetPeriod = 25;
    private const byte FailuresAfterReset = 1;
    private const int PartialMinuteSeconds = 30;
    private const int MinutesAfterLockEnds = 6;
    private const int NoMinutesLeft = 0;

    private static readonly DateTime _now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Activate_ActiveAccount_ThrowsInvalidOperationException()
    {
        Account account = CreateActiveAccount();

        Action activate = account.Activate;

        Assert.Throws<InvalidOperationException>(activate);
    }

    [Fact]
    public void RecordFailedLogin_TwoFailures_DoesNotLock()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, FailuresBelowLock);

        bool isLocked = account.IsLockedAt(_now);

        Assert.False(isLocked);
    }

    [Fact]
    public void RecordFailedLogin_ThirdFailure_LocksForFiveMinutes()
    {
        Account account = CreateActiveAccount();
        DateTime expectedLockEnd = _now.AddMinutes(FirstLockMinutes);

        RecordFailures(account, FailuresThatLock);

        Assert.Equal(expectedLockEnd, account.LockedUntil);
    }

    [Fact]
    public void RecordFailedLogin_AfterResetPeriod_RestartsCounter()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, FailuresBelowLock);
        DateTime afterResetPeriod = _now.AddHours(HoursAfterResetPeriod);

        account.RecordFailedLogin(afterResetPeriod);

        Assert.Equal(FailuresAfterReset, account.FailedLoginCount);
    }

    [Fact]
    public void RecordSuccessfulLogin_LockedAccount_RemovesLock()
    {
        Account account = CreateLockedAccount();

        account.RecordSuccessfulLogin();

        bool isLocked = account.IsLockedAt(_now);
        Assert.False(isLocked);
    }

    [Fact]
    public void GetRemainingLockMinutes_PartialMinuteLeft_RoundsUp()
    {
        Account account = CreateLockedAccount();
        DateTime partialMinuteLater = _now.AddSeconds(PartialMinuteSeconds);

        int remainingMinutes = account.GetRemainingLockMinutes(partialMinuteLater);

        Assert.Equal(FirstLockMinutes, remainingMinutes);
    }

    [Fact]
    public void GetRemainingLockMinutes_LockExpired_ReturnsZero()
    {
        Account account = CreateLockedAccount();
        DateTime afterLockEnds = _now.AddMinutes(MinutesAfterLockEnds);

        int remainingMinutes = account.GetRemainingLockMinutes(afterLockEnds);

        Assert.Equal(NoMinutesLeft, remainingMinutes);
    }

    [Fact]
    public void RecordLastSeen_AnyMoment_StoresMoment()
    {
        Account account = CreateActiveAccount();

        account.RecordLastSeen(_now);

        Assert.Equal(_now, account.LastSeenAt);
    }

    private static Account CreateActiveAccount()
    {
        var account = new Account(Nickname, Email);
        account.Activate();
        return account;
    }

    private static void RecordFailures(Account account, int failureCount)
    {
        for (int failureIndex = 0; failureIndex < failureCount; failureIndex++)
        {
            account.RecordFailedLogin(_now);
        }
    }

    private static Account CreateLockedAccount()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, FailuresThatLock);
        return account;
    }
}
