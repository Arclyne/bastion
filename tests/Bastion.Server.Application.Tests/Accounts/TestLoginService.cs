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
    public void LogIn_UnknownIdentifier_ReturnsInvalidCredentials()
    {
        LoginOutcome outcome = _service.LogIn(CreateAttempt("nadie"), Password);

        Assert.Equal(LoginOutcomeCode.InvalidCredentials, outcome.Code);
    }

    [Fact]
    public void LogIn_UnknownIdentifier_StillChecksAPassword()
    {
        _service.LogIn(CreateAttempt("nadie"), Password);

        Assert.Equal(1, _passwordHasher.VerifyCount);
    }

    [Fact]
    public void LogIn_UnknownIdentifier_RecordsAccountNotFound()
    {
        _service.LogIn(CreateAttempt("nadie"), Password);

        Assert.Equal(AccessLogResult.AccountNotFound, _unitOfWork.AccessLogStore.AccessLogs[0].Result);
    }

    [Fact]
    public void LogIn_WrongPassword_ReturnsInvalidCredentials()
    {
        StoreActiveAccount();

        LoginOutcome outcome = _service.LogIn(CreateAttempt(Nickname), WrongPassword);

        Assert.Equal(LoginOutcomeCode.InvalidCredentials, outcome.Code);
    }

    [Fact]
    public void LogIn_WrongPassword_CountsTheFailure()
    {
        Account account = StoreActiveAccount();

        _service.LogIn(CreateAttempt(Nickname), WrongPassword);

        Assert.Equal(1, account.FailedAttempts);
    }

    [Fact]
    public void LogIn_ThirdWrongPassword_ReturnsAccountLockedForFiveMinutes()
    {
        StoreActiveAccount();
        _service.LogIn(CreateAttempt(Nickname), WrongPassword);
        _service.LogIn(CreateAttempt(Nickname), WrongPassword);

        LoginOutcome outcome = _service.LogIn(CreateAttempt(Nickname), WrongPassword);

        Assert.Equal(LoginOutcome.Locked(5), outcome);
    }

    [Fact]
    public void LogIn_LockedAccountWithRightPassword_ReturnsAccountLocked()
    {
        Account account = StoreActiveAccount();
        LockAccount(account);

        LoginOutcome outcome = _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(LoginOutcomeCode.AccountLocked, outcome.Code);
    }

    [Fact]
    public void LogIn_LockedAccount_DoesNotExtendTheLock()
    {
        Account account = StoreActiveAccount();
        LockAccount(account);
        DateTime? lockedUntil = account.LockedUntil;

        _service.LogIn(CreateAttempt(Nickname), WrongPassword);

        Assert.Equal(lockedUntil, account.LockedUntil);
    }

    [Fact]
    public void LogIn_PendingAccount_ReturnsAccountPending()
    {
        var account = new Account(Nickname, Email);
        account.ChangePassword(FakePasswordHasher.HashOf(Password), new byte[] { 9 });
        _unitOfWork.AccountStore.Store(account);

        LoginOutcome outcome = _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(LoginOutcomeCode.AccountPending, outcome.Code);
    }

    [Fact]
    public void LogIn_ValidCredentials_ReturnsTheSessionToken()
    {
        StoreActiveAccount();

        LoginOutcome outcome = _service.LogIn(CreateAttempt(Nickname), Password);

        Assert.Equal(FakeSessionTokenGenerator.Token, outcome.Player?.SessionToken);
    }

    [Fact]
    public void LogIn_ValidEmail_ReturnsSuccess()
    {
        StoreActiveAccount();

        LoginOutcome outcome = _service.LogIn(CreateAttempt(Email), Password);

        Assert.Equal(LoginOutcomeCode.Success, outcome.Code);
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
        _service.LogIn(CreateAttempt(Nickname), WrongPassword);

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
}
