using System;
using Bastion.Domain.Accounts;
using Xunit;

namespace Bastion.Domain.Tests.Accounts;

public class TestAccount
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";

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
        RecordFailures(account, 2);

        bool isLocked = account.IsLockedAt(_now);

        Assert.False(isLocked);
    }

    [Fact]
    public void RecordFailedLogin_ThirdFailure_LocksForFiveMinutes()
    {
        Account account = CreateActiveAccount();

        RecordFailures(account, 3);

        Assert.Equal(_now.AddMinutes(5), account.LockedUntil);
    }

    [Fact]
    public void RecordFailedLogin_AfterResetPeriod_RestartsCounter()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 2);

        account.RecordFailedLogin(_now.AddHours(25));

        Assert.Equal(1, account.FailedLoginCount);
    }

    [Fact]
    public void RecordSuccessfulLogin_LockedAccount_RemovesLock()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 3);

        account.RecordSuccessfulLogin();

        Assert.False(account.IsLockedAt(_now));
    }

    [Fact]
    public void GetRemainingLockMinutes_PartialMinuteLeft_RoundsUp()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 3);

        int remainingMinutes = account.GetRemainingLockMinutes(_now.AddSeconds(30));

        Assert.Equal(5, remainingMinutes);
    }

    [Fact]
    public void GetRemainingLockMinutes_LockExpired_ReturnsZero()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 3);

        int remainingMinutes = account.GetRemainingLockMinutes(_now.AddMinutes(6));

        Assert.Equal(0, remainingMinutes);
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
}
