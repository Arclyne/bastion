using System;
using log4net;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Accounts;

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

    public LoginOutcome LogIn(AccessAttempt attempt, string password)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(password);

        Account? account = _unitOfWork.Accounts.FindByIdentifier(attempt.EnteredIdentifier);
        if (account is null || !account.HasPassword)
        {
            _passwordHasher.Verify(password, null, null);
            return Finish(
                new AccessLog(AccessLogResult.AccountNotFound, attempt, null),
                LoginOutcome.Failed(LoginOutcomeCode.InvalidCredentials));
        }

        if (account.IsLockedAt(attempt.OccurredAt))
        {
            return Finish(
                new AccessLog(AccessLogResult.AccountLocked, attempt, account.AccountId),
                LoginOutcome.Locked(account.GetRemainingLockMinutes(attempt.OccurredAt)));
        }

        if (!_passwordHasher.Verify(password, account.PasswordHash, account.PasswordSalt))
        {
            return RejectPassword(account, attempt);
        }

        account.RecordSuccessfulSignIn();
        return Admit(account, attempt);
    }

    private LoginOutcome RejectPassword(Account account, AccessAttempt attempt)
    {
        account.RecordFailedSignIn(attempt.OccurredAt);
        bool isNowLocked = account.IsLockedAt(attempt.OccurredAt);
        LoginOutcome outcome = isNowLocked
            ? LoginOutcome.Locked(account.GetRemainingLockMinutes(attempt.OccurredAt))
            : LoginOutcome.Failed(LoginOutcomeCode.InvalidCredentials);
        return Finish(new AccessLog(AccessLogResult.WrongPassword, attempt, account.AccountId), outcome);
    }

    private LoginOutcome Admit(Account account, AccessAttempt attempt)
    {
        switch (account.AccountStatus)
        {
            case AccountStatus.Active:
                return OpenSession(account, attempt);
            case AccountStatus.Pending:
                return Finish(
                    new AccessLog(AccessLogResult.AccountPending, attempt, account.AccountId),
                    LoginOutcome.Failed(LoginOutcomeCode.AccountPending));
            case AccountStatus.Suspended:
                return Finish(
                    new AccessLog(AccessLogResult.AccountSanctioned, attempt, account.AccountId),
                    LoginOutcome.Failed(LoginOutcomeCode.AccountSuspended));
            case AccountStatus.Banned:
                return Finish(
                    new AccessLog(AccessLogResult.AccountSanctioned, attempt, account.AccountId),
                    LoginOutcome.Failed(LoginOutcomeCode.AccountBanned));
            default:
                return Finish(
                    new AccessLog(AccessLogResult.AccountNotFound, attempt, account.AccountId),
                    LoginOutcome.Failed(LoginOutcomeCode.InvalidCredentials));
        }
    }

    private LoginOutcome OpenSession(Account account, AccessAttempt attempt)
    {
        GeneratedToken token = _tokenGenerator.Generate();
        var ticket = new SessionTicket(token.TokenHash, attempt.OccurredAt);
        _unitOfWork.Sessions.Add(new Session(account.AccountId, ticket, attempt.Connection));

        var player = new SignedInPlayer(token.Token, account.Nickname, account.PreferredLanguage);
        return Finish(
            new AccessLog(AccessLogResult.LoginSucceeded, attempt, account.AccountId),
            LoginOutcome.Succeeded(player));
    }

    private LoginOutcome Finish(AccessLog accessLog, LoginOutcome outcome)
    {
        _unitOfWork.AccessLogs.Add(accessLog);
        _unitOfWork.SaveChanges();
        _logger.Info($"Sign-in attempt processed. Result={accessLog.Result}; AccountId={accessLog.AccountId}");
        return outcome;
    }
}
