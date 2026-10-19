namespace Bastion.Domain.Auditing;

public enum AccessLogResult
{
    LoginSucceeded,
    AccountNotFound,
    AccountLocked,
    WrongPassword,
    AccountPending,
    AccountSanctioned,
}
