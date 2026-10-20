using System;
using CoreWCF;
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

    public static PlayerSession ToSession(SignedInPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new PlayerSession
        {
            Token = player.SessionToken,
            Nickname = player.Nickname,
            PreferredLanguage = player.PreferredLanguage,
        };
    }

    public static FaultException<InvalidCredentialsFault> ToFault(InvalidCredentialsException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        return new FaultException<InvalidCredentialsFault>(
            new InvalidCredentialsFault(),
            new FaultReason(rejection.Message));
    }

    public static FaultException<AccountRestrictedFault> ToFault(AccountRestrictedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new AccountRestrictedFault
        {
            Reason = ToReason(rejection.Restriction),
            LockMinutes = rejection.LockMinutes,
        };
        return new FaultException<AccountRestrictedFault>(fault, new FaultReason(rejection.Message));
    }

    private static RestrictionReason ToReason(AccountRestriction restriction)
    {
        switch (restriction)
        {
            case AccountRestriction.Locked:
                return RestrictionReason.Locked;
            case AccountRestriction.Pending:
                return RestrictionReason.Pending;
            case AccountRestriction.Suspended:
                return RestrictionReason.Suspended;
            default:
                return RestrictionReason.Banned;
        }
    }
}
