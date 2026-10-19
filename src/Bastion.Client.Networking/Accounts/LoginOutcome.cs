namespace Bastion.Client.Networking.Accounts;

public sealed record LoginOutcome(LoginStatus Status, SignedInSession? Session, int LockMinutes)
{
    public static LoginOutcome Failed(LoginStatus status)
    {
        return new LoginOutcome(status, null, 0);
    }
}
