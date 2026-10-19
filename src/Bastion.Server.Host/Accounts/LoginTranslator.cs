using System;
using Bastion.Contracts.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Host.Accounts;

public static class LoginTranslator
{
    public static ClientConnection ToConnection(ClientDevice device, string ipAddress)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(ipAddress);

        return new ClientConnection(
            ipAddress,
            device.DeviceFingerprint ?? string.Empty,
            device.ClientVersion ?? string.Empty);
    }

    public static AccessAttempt ToAttempt(PlayerCredentials credentials, ClientConnection connection, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(connection);

        return new AccessAttempt((credentials.Identifier ?? string.Empty).Trim(), connection, now);
    }

    public static AccessDecision ToDecision(LoginOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        return new AccessDecision
        {
            Code = ToDecisionCode(outcome.Code),
            Session = ToSession(outcome.Player),
            LockMinutes = outcome.LockMinutes,
        };
    }

    public static AccessDecision Deny(AccessDecisionCode code)
    {
        return new AccessDecision { Code = code };
    }

    private static PlayerSession? ToSession(SignedInPlayer? player)
    {
        if (player is null)
        {
            return null;
        }

        return new PlayerSession
        {
            Token = player.SessionToken,
            Nickname = player.Nickname,
            PreferredLanguage = player.PreferredLanguage,
        };
    }

    private static AccessDecisionCode ToDecisionCode(LoginOutcomeCode code)
    {
        switch (code)
        {
            case LoginOutcomeCode.Success:
                return AccessDecisionCode.Granted;
            case LoginOutcomeCode.InvalidCredentials:
                return AccessDecisionCode.InvalidCredentials;
            case LoginOutcomeCode.AccountLocked:
                return AccessDecisionCode.AccountLocked;
            case LoginOutcomeCode.AccountPending:
                return AccessDecisionCode.AccountPending;
            case LoginOutcomeCode.AccountSuspended:
                return AccessDecisionCode.AccountSuspended;
            case LoginOutcomeCode.AccountBanned:
                return AccessDecisionCode.AccountBanned;
            default:
                return AccessDecisionCode.ServerError;
        }
    }
}
