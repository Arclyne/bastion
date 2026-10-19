namespace Bastion.Server.Application.Accounts;

public sealed record LoginOutcome(LoginOutcomeCode Code, SignedInPlayer? Player, int LockMinutes)
{
    public static LoginOutcome Succeeded(SignedInPlayer player)
    {
        return new LoginOutcome(LoginOutcomeCode.Success, player, 0);
    }

    public static LoginOutcome Failed(LoginOutcomeCode code)
    {
        return new LoginOutcome(code, null, 0);
    }

    public static LoginOutcome Locked(int lockMinutes)
    {
        return new LoginOutcome(LoginOutcomeCode.AccountLocked, null, lockMinutes);
    }
}
