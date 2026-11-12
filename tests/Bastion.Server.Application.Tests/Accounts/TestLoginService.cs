using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Accounts;

public sealed class TestLoginService
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";
    private const string Password = "Secreta#2026";
    private const string WrongPassword = "Incorrecta#1";
    private const string UnknownIdentifier = "nadie";
    private const int FailuresThatLock = 3;
    private const int FirstLockMinutes = 5;
    private const int OneSimulatedCheck = 1;
    private const byte OneFailure = 1;
    private const byte NoFailures = 0;
    private const int OneSave = 1;

    private static readonly DateTime _now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");
    private static readonly byte[] _salt = { 9 };

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakePasswordHasher _passwordHasher = new FakePasswordHasher();
    private readonly FakeSessionTokenGenerator _tokenGenerator = new FakeSessionTokenGenerator();
    private readonly LoginService _loginService;

    public TestLoginService()
    {
        _loginService = new LoginService(_unitOfWork, _passwordHasher, _tokenGenerator);
    }

    [Fact]
    public void LogIn_UnknownIdentifier_ThrowsInvalidCredentials()
    {
        AccessAttempt attempt = CreateAttempt(UnknownIdentifier);

        Action logIn = () => _loginService.LogIn(attempt, Password);

        Assert.Throws<InvalidCredentialsException>(logIn);
    }

    [Fact]
    public void LogIn_UnknownIdentifier_SimulatesAPasswordCheck()
    {
        AccessAttempt attempt = CreateAttempt(UnknownIdentifier);
        Action logIn = () => _loginService.LogIn(attempt, Password);

        Record.Exception(logIn);

        Assert.Equal(OneSimulatedCheck, _passwordHasher.SimulatedCheckCount);
    }

    [Fact]
    public void LogIn_UnknownIdentifier_RecordsAccountNotFound()
    {
        AccessAttempt attempt = CreateAttempt(UnknownIdentifier);
        Action logIn = () => _loginService.LogIn(attempt, Password);

        Record.Exception(logIn);

        AccessLog accessLog = _unitOfWork.AccessLogRepository.AccessLogs[0];
        Assert.Equal(AccessLogResult.AccountNotFound, accessLog.Result);
    }

    [Fact]
    public void LogIn_WrongPassword_ThrowsInvalidCredentials()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);

        Action logIn = () => _loginService.LogIn(attempt, WrongPassword);

        Assert.Throws<InvalidCredentialsException>(logIn);
    }

    [Fact]
    public void LogIn_WrongPassword_CountsTheFailure()
    {
        Account account = AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);
        Action logIn = () => _loginService.LogIn(attempt, WrongPassword);

        Record.Exception(logIn);

        Assert.Equal(OneFailure, account.FailedLoginCount);
    }

    [Fact]
    public void LogIn_ThirdWrongPassword_ThrowsAccountLockedForFiveMinutes()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);
        Action logIn = () => _loginService.LogIn(attempt, WrongPassword);
        Record.Exception(logIn);
        Record.Exception(logIn);

        Exception? exception = Record.Exception(logIn);

        var rejection = exception as AccountRestrictedException;
        Assert.Equal(FirstLockMinutes, rejection?.LockMinutes);
    }

    [Fact]
    public void LogIn_LockedAccountWithRightPassword_ThrowsAccountLocked()
    {
        Account account = AddActiveAccount();
        LockAccount(account);
        AccessAttempt attempt = CreateAttempt(Nickname);
        Action logIn = () => _loginService.LogIn(attempt, Password);

        Exception? exception = Record.Exception(logIn);

        var rejection = exception as AccountRestrictedException;
        Assert.Equal(AccountRestriction.Locked, rejection?.Restriction);
    }

    [Fact]
    public void LogIn_LockedAccount_DoesNotExtendTheLock()
    {
        Account account = AddActiveAccount();
        LockAccount(account);
        DateTime? expectedLockEnd = account.LockedUntil;
        AccessAttempt attempt = CreateAttempt(Nickname);
        Action logIn = () => _loginService.LogIn(attempt, WrongPassword);

        Record.Exception(logIn);

        Assert.Equal(expectedLockEnd, account.LockedUntil);
    }

    [Fact]
    public void LogIn_PendingAccount_ThrowsAccountPending()
    {
        AddPendingAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);
        Action logIn = () => _loginService.LogIn(attempt, Password);

        Exception? exception = Record.Exception(logIn);

        var rejection = exception as AccountRestrictedException;
        Assert.Equal(AccountRestriction.Pending, rejection?.Restriction);
    }

    [Fact]
    public void LogIn_ValidCredentials_ReturnsTheSessionToken()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);

        LoggedInPlayer player = _loginService.LogIn(attempt, Password);

        Assert.Equal(FakeSessionTokenGenerator.Token, player.SessionToken);
    }

    [Fact]
    public void LogIn_ValidEmail_ReturnsThePlayerNickname()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Email);

        LoggedInPlayer player = _loginService.LogIn(attempt, Password);

        Assert.Equal(Nickname, player.Nickname);
    }

    [Fact]
    public void LogIn_ValidCredentials_OpensASessionThatExpiresInADay()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);
        DateTime expectedExpiry = _now.AddHours(Session.MaximumLifetimeHours);

        _loginService.LogIn(attempt, Password);

        Session session = _unitOfWork.SessionRepository.Sessions[0];
        Assert.Equal(expectedExpiry, session.ExpiresAt);
    }

    [Fact]
    public void LogIn_ValidCredentials_ResetsFailedLoginCount()
    {
        Account account = AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);
        Action logInWithWrongPassword = () => _loginService.LogIn(attempt, WrongPassword);
        Record.Exception(logInWithWrongPassword);

        _loginService.LogIn(attempt, Password);

        Assert.Equal(NoFailures, account.FailedLoginCount);
    }

    [Fact]
    public void LogIn_ValidCredentials_SavesEverythingOnce()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);

        _loginService.LogIn(attempt, Password);

        Assert.Equal(OneSave, _unitOfWork.SaveCount);
    }

    [Fact]
    public void LogIn_ValidCredentials_RecordsLoginSucceeded()
    {
        AddActiveAccount();
        AccessAttempt attempt = CreateAttempt(Nickname);

        _loginService.LogIn(attempt, Password);

        AccessLog accessLog = _unitOfWork.AccessLogRepository.AccessLogs[0];
        Assert.Equal(AccessLogResult.LoginSucceeded, accessLog.Result);
    }

    private static AccessAttempt CreateAttempt(string identifier)
    {
        return new AccessAttempt(identifier, _connection, _now);
    }

    private static void LockAccount(Account account)
    {
        for (int failureIndex = 0; failureIndex < FailuresThatLock; failureIndex++)
        {
            account.RecordFailedLogin(_now);
        }
    }

    private Account AddActiveAccount()
    {
        Account account = AddPendingAccount();
        account.Activate();
        return account;
    }

    private Account AddPendingAccount()
    {
        var account = new Account(Nickname, Email);
        byte[] passwordHash = FakePasswordHasher.HashOf(Password);
        account.ChangePassword(passwordHash, _salt);
        _unitOfWork.AccountRepository.AddAccount(account);
        return account;
    }
}
