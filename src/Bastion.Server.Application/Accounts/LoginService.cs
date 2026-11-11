using System;
using System.Diagnostics.CodeAnalysis;
using log4net;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Accounts;

/// <inheritdoc/>
public sealed class LoginService : ILoginService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginService));

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionTokenGenerator _tokenGenerator;

    public LoginService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ISessionTokenGenerator tokenGenerator)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(tokenGenerator);

        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    /// <inheritdoc/>
    public LoggedInPlayer LogIn(AccessAttempt attempt, string password)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(password);

        Account? account = _unitOfWork.Accounts.FindByLoginIdentifier(attempt.EnteredIdentifier);
        if (account is null || !account.HasPassword())
        {
            _passwordHasher.SimulatePasswordCheck(password);
            throw Reject(
                new AccessLog(AccessLogResult.AccountNotFound, attempt, null),
                new InvalidCredentialsException());
        }

        if (account.IsLockedAt(attempt.OccurredAt))
        {
            throw Reject(
                new AccessLog(AccessLogResult.AccountLocked, attempt, account.AccountId),
                CreateLockRestriction(account, attempt.OccurredAt));
        }

        if (!_passwordHasher.IsPasswordCorrect(password, account.PasswordHash, account.PasswordSalt))
        {
            RejectPassword(account, attempt);
        }

        account.RecordSuccessfulLogin();
        return AdmitByAccountStatus(account, attempt);
    }

    private static AccountRestrictedException CreateLockRestriction(Account account, DateTime now)
    {
        int remainingLockMinutes = account.GetRemainingLockMinutes(now);
        return new AccountRestrictedException(AccountRestriction.Locked, remainingLockMinutes);
    }

    [DoesNotReturn]
    private void RejectPassword(Account account, AccessAttempt attempt)
    {
        account.RecordFailedLogin(attempt.OccurredAt);
        var accessLog = new AccessLog(AccessLogResult.WrongPassword, attempt, account.AccountId);
        if (account.IsLockedAt(attempt.OccurredAt))
        {
            throw Reject(accessLog, CreateLockRestriction(account, attempt.OccurredAt));
        }

        throw Reject(accessLog, new InvalidCredentialsException());
    }

    private LoggedInPlayer AdmitByAccountStatus(Account account, AccessAttempt attempt)
    {
        switch (account.AccountStatus)
        {
            case AccountStatus.Active:
                return OpenSession(account, attempt);
            case AccountStatus.Pending:
                throw Reject(
                    new AccessLog(AccessLogResult.AccountPending, attempt, account.AccountId),
                    new AccountRestrictedException(AccountRestriction.Pending));
            case AccountStatus.Suspended:
                throw Reject(
                    new AccessLog(AccessLogResult.AccountSanctioned, attempt, account.AccountId),
                    new AccountRestrictedException(AccountRestriction.Suspended));
            case AccountStatus.Banned:
                throw Reject(
                    new AccessLog(AccessLogResult.AccountSanctioned, attempt, account.AccountId),
                    new AccountRestrictedException(AccountRestriction.Banned));
            case AccountStatus.Deleted:
                throw Reject(
                    new AccessLog(AccessLogResult.AccountNotFound, attempt, account.AccountId),
                    new InvalidCredentialsException());
            default:
                throw new InvalidOperationException(
                    $"Unsupported account status. AccountStatus={account.AccountStatus}");
        }
    }

    private LoggedInPlayer OpenSession(Account account, AccessAttempt attempt)
    {
        GeneratedToken token = _tokenGenerator.Generate();
        var ticket = new SessionTicket(token.TokenHash, attempt.OccurredAt);
        _unitOfWork.Sessions.Add(new Session(account.AccountId, ticket, attempt.Connection));
        RecordAccessLog(new AccessLog(AccessLogResult.LoginSucceeded, attempt, account.AccountId));
        return new LoggedInPlayer(token.Token, account.Nickname, account.PreferredLanguage);
    }

    private TException Reject<TException>(AccessLog accessLog, TException rejection)
        where TException : Exception
    {
        RecordAccessLog(accessLog);
        return rejection;
    }

    private void RecordAccessLog(AccessLog accessLog)
    {
        _unitOfWork.AccessLogs.Add(accessLog);
        _unitOfWork.SaveChanges();
        string message = $"Login attempt processed. Result={accessLog.Result}; AccountId={accessLog.AccountId}";
        if (accessLog.Result == AccessLogResult.LoginSucceeded)
        {
            _logger.Info(message);
            return;
        }

        _logger.Warn(message);
    }
}
