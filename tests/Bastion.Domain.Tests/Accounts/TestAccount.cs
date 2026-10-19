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
    public void CanSignIn_NewAccount_IsFalse()
    {
        var account = new Account(Nickname, Email);

        bool canSignIn = account.CanSignIn;

        Assert.False(canSignIn);
    }

    [Fact]
    public void CanSignIn_ActivatedAccount_IsTrue()
    {
        Account account = CreateActiveAccount();

        bool canSignIn = account.CanSignIn;

        Assert.True(canSignIn);
    }

    [Fact]
    public void Activate_ActiveAccount_ThrowsInvalidOperationException()
    {
        Account account = CreateActiveAccount();

        Action activate = account.Activate;

        Assert.Throws<InvalidOperationException>(activate);
    }

    [Fact]
    public void RecordFailedSignIn_TwoFailures_DoesNotLock()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 2);

        bool isLocked = account.IsLockedAt(_now);

        Assert.False(isLocked);
    }

    [Fact]
    public void RecordFailedSignIn_ThirdFailure_LocksForFiveMinutes()
    {
        Account account = CreateActiveAccount();

        RecordFailures(account, 3);

        Assert.Equal(_now.AddMinutes(5), account.LockedUntil);
    }

    [Fact]
    public void RecordFailedSignIn_AfterResetPeriod_RestartsCounter()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 2);

        account.RecordFailedSignIn(_now.AddHours(25));

        Assert.Equal(1, account.FailedAttempts);
    }

    [Fact]
    public void RecordSuccessfulSignIn_LockedAccount_RemovesLock()
    {
        Account account = CreateActiveAccount();
        RecordFailures(account, 3);

        account.RecordSuccessfulSignIn();

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
            account.RecordFailedSignIn(_now);
        }
    }
}
