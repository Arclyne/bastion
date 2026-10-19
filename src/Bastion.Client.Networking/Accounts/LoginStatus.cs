namespace Bastion.Client.Networking.Accounts;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountLocked,
    AccountPending,
    AccountSuspended,
    AccountBanned,
    ServerError,
    ServerUnreachable,
    ConnectionLost,
}
