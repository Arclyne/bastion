namespace Bastion.Server.Application.Accounts;

public enum LoginOutcomeCode
{
    Success,
    InvalidCredentials,
    AccountLocked,
    AccountPending,
    AccountSuspended,
    AccountBanned,
}
