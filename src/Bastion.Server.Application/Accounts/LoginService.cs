using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Common;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Security;
using log4net;

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

        Lookup<Account> foundAccount = _unitOfWork.Accounts.FindByLoginIdentifier(attempt.EnteredIdentifier);
        if (!foundAccount.IsFound)
        {
            throw RejectUnknownAccount(password, attempt, accountId: null);
        }

        Account account = foundAccount.GetValue();
        if (!account.HasPassword())
        {
            throw RejectUnknownAccount(password, attempt, account.AccountId);
        }

        if (account.IsLockedAt(attempt.OccurredAt))
        {
            RecordAccessLog(AccessLogResult.AccountLocked, attempt, account.AccountId);
            throw CreateLockRestriction(account, attempt.OccurredAt);
        }

        if (!_passwordHasher.IsPasswordCorrect(password, account.PasswordHash, account.PasswordSalt))
        {
            throw RejectPassword(account, attempt);
        }

        account.RecordSuccessfulLogin();
        return AdmitByAccountStatus(account, attempt);
    }

    private InvalidCredentialsException RejectUnknownAccount(string password, AccessAttempt attempt, int? accountId)
    {
        _passwordHasher.SimulatePasswordCheck(password);
        RecordAccessLog(AccessLogResult.AccountNotFound, attempt, accountId);
        return new InvalidCredentialsException();
    }

    private static AccountRestrictedException CreateLockRestriction(Account account, DateTime now)
    {
        int remainingLockMinutes = account.GetRemainingLockMinutes(now);
        return new AccountRestrictedException(AccountRestriction.Locked, remainingLockMinutes);
    }

    private Exception RejectPassword(Account account, AccessAttempt attempt)
    {
        account.RecordFailedLogin(attempt.OccurredAt);
        RecordAccessLog(AccessLogResult.WrongPassword, attempt, account.AccountId);
        if (account.IsLockedAt(attempt.OccurredAt))
        {
            return CreateLockRestriction(account, attempt.OccurredAt);
        }

        return new InvalidCredentialsException();
    }

    private LoggedInPlayer AdmitByAccountStatus(Account account, AccessAttempt attempt)
    {
        switch (account.AccountStatus)
        {
            case AccountStatus.Active:
                return OpenSession(account, attempt);
            case AccountStatus.Pending:
                RecordAccessLog(AccessLogResult.AccountPending, attempt, account.AccountId);
                throw new AccountRestrictedException(AccountRestriction.Pending);
            case AccountStatus.Suspended:
                RecordAccessLog(AccessLogResult.AccountSanctioned, attempt, account.AccountId);
                throw new AccountRestrictedException(AccountRestriction.Suspended);
            case AccountStatus.Banned:
                RecordAccessLog(AccessLogResult.AccountSanctioned, attempt, account.AccountId);
                throw new AccountRestrictedException(AccountRestriction.Banned);
            case AccountStatus.Deleted:
                RecordAccessLog(AccessLogResult.AccountNotFound, attempt, account.AccountId);
                throw new InvalidCredentialsException();
            default:
                throw new InvalidOperationException(
                    $"Unsupported account status. AccountStatus={account.AccountStatus}");
        }
    }

    private LoggedInPlayer OpenSession(Account account, AccessAttempt attempt)
    {
        GeneratedToken token = _tokenGenerator.Generate();
        var ticket = new SessionTicket(token.TokenHash, attempt.OccurredAt);
        var session = new Session(account.AccountId, ticket, attempt.Connection);
        _unitOfWork.Sessions.Add(session);
        RecordAccessLog(AccessLogResult.LoginSucceeded, attempt, account.AccountId);
        return new LoggedInPlayer(token.Token, account.Nickname, account.PreferredLanguage);
    }

    private void RecordAccessLog(AccessLogResult result, AccessAttempt attempt, int? accountId)
    {
        var accessLog = new AccessLog(result, attempt, accountId);
        _unitOfWork.AccessLogs.Add(accessLog);
        _unitOfWork.SaveChanges();
        string message = $"Login attempt processed. Result={result}; AccountId={accountId}";
        if (result == AccessLogResult.LoginSucceeded)
        {
            _logger.Info(message);
            return;
        }

        _logger.Warn(message);
    }
}
