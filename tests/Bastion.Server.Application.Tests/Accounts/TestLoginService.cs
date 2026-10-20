using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Accounts;

public class TestLoginService
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";
    private const string Password = "Secreta#2026";
    private const string WrongPassword = "Incorrecta#1";
    private const int FailuresThatLock = 3;

    private static readonly DateTime _now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakePasswordHasher _passwordHasher = new FakePasswordHasher();
    private readonly LoginService _service;

    public TestLoginService()
    {
        _service = new LoginService(_unitOfWork, _passwordHasher, new FakeSessionTokenGenerator());
    }

    [Fact]
    public void LogIn_UnknownIdentifier_ThrowsInvalidCredentials()
    {
        Assert.Throws<InvalidCredentialsException>(() => _service.LogIn(CreateAttempt("nadie"), Password));
    }

    [Fact]
    public void LogIn_UnknownIdentifier_StillChecksAPassword()
    {
        TryLogIn("nadie", Password);

        Assert.Equal(1, _passwordHasher.VerifyCount);
    }

    [Fact]
    public void LogIn_UnknownIdentifier_RecordsAccountNotFound()
    {
        TryLogIn("nadie", Password);

        Assert.Equal(AccessLogResult.AccountNotFound, _unitOfWork.AccessLogStore.AccessLogs[0].Result);
    }

    [Fact]
    public void LogIn_WrongPassword_ThrowsInvalidCredentials()
    {
        StoreActiveAccount();

        Assert.Throws<InvalidCredentialsException>(() => _service.LogIn(CreateAttempt(Nickname), WrongPassword));
    }

    [Fact]
    public void LogIn_WrongPassword_CountsTheFailure()
    {
        Account account = StoreActiveAccount();

        TryLogIn(Nickname, WrongPassword);

        Assert.Equal(1, account.FailedAttempts);
    }

    [Fact]
    public void LogIn_ThirdWrongPassword_ThrowsAccountLockedForFiveMinutes()
    {
        StoreActiveAccount();
        TryLogIn(Nickname, WrongPassword);
        TryLogIn(Nickname, WrongPassword);

        var rejection =
            Record.Exception(() => _service.LogIn(CreateAttempt(Nickname), WrongPassword)) as AccountRestrictedException;

        Assert.Equal(5, rejection?.LockMinutes);
    }

    [Fact]
    public void LogIn_LockedAccountWithRightPassword_ThrowsAccountLocked()
    {
        Account account = StoreActiveAccount();
        LockAccount(account);

        var rejection =
            Record.Exception(() => _service.LogIn(CreateAttempt(Nickname), Password)) as AccountRestrictedException;

        Assert.Equal(AccountRestriction.Locked, rejection?.Restriction);
    }

    [Fact]
    public void LogIn_LockedAccount_DoesNotExtendTheLock()
    {
        Account account = StoreActiveAccount();
        LockAccount(account);
        DateTime? lockedUntil = account.LockedUntil;

        TryLogIn(Nickname, WrongPassword);

        Assert.Equal(lockedUntil, account.LockedUntil);
    }

    [Fact]
    public void LogIn_PendingAccount_ThrowsAccountPending()
    {
        var account = new Account(Nickname, Email);
        account.ChangePassword(FakePasswordHasher.HashOf(Password), new byte[] { 9 });
        _unitOfWork.AccountStore.Store(account);

        var rejection =
            Record.Exception(() => _service.LogIn(CreateAttempt(Nickname), Password)) as AccountRestrictedException;

        Assert.Equal(AccountRestriction.Pending, rejection?.Restriction);
    }

    [Fact]
    public void LogIn_ValidCredentials_ReturnsTheSessionToken()
    {
        StoreActiveAccount();

        SignedInPlayer player = _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(FakeSessionTokenGenerator.Token, player.SessionToken);
    }

    [Fact]
    public void LogIn_ValidEmail_ReturnsThePlayerNickname()
    {
        StoreActiveAccount();

        SignedInPlayer player = _service.LogIn(CreateAttempt(Email), Password);

        Assert.Equal(Nickname, player.Nickname);
    }

    [Fact]
    public void LogIn_ValidCredentials_OpensASessionThatExpiresInADay()
    {
        StoreActiveAccount();

        _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(_now.AddHours(24), _unitOfWork.SessionStore.Sessions[0].ExpiresAt);
    }

    [Fact]
    public void LogIn_ValidCredentials_ResetsFailedAttempts()
    {
        Account account = StoreActiveAccount();
        TryLogIn(Nickname, WrongPassword);

        _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(0, account.FailedAttempts);
    }

    [Fact]
    public void LogIn_ValidCredentials_SavesEverythingOnce()
    {
        StoreActiveAccount();

        _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public void LogIn_ValidCredentials_RecordsLoginSucceeded()
    {
        StoreActiveAccount();

        _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(AccessLogResult.LoginSucceeded, _unitOfWork.AccessLogStore.AccessLogs[0].Result);
    }

    private static AccessAttempt CreateAttempt(string identifier)
    {
        return new AccessAttempt(identifier, _connection, _now);
    }

    private static void LockAccount(Account account)
    {
        for (int failureIndex = 0; failureIndex < FailuresThatLock; failureIndex++)
        {
            account.RecordFailedSignIn(_now);
        }
    }

    private Account StoreActiveAccount()
    {
        var account = new Account(Nickname, Email);
        account.ChangePassword(FakePasswordHasher.HashOf(Password), new byte[] { 9 });
        account.Activate();
        _unitOfWork.AccountStore.Store(account);
        return account;
    }

    private void TryLogIn(string identifier, string password)
    {
        Record.Exception(() => _service.LogIn(CreateAttempt(identifier), password));
    }
}
