using System;
using Bastion.Contracts.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;
using CoreWCF;

namespace Bastion.Server.Host.Accounts;

public static class LoginTranslator
{
    public static ClientConnection ToConnection(ClientDevice device, string ipAddress)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(ipAddress);

        string deviceFingerprint = device.DeviceFingerprint ?? string.Empty;
        string clientVersion = device.ClientVersion ?? string.Empty;
        return new ClientConnection(ipAddress, deviceFingerprint, clientVersion);
    }

    public static AccessAttempt ToAttempt(PlayerCredentials credentials, ClientConnection connection, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(connection);

        string enteredIdentifier = (credentials.Identifier ?? string.Empty).Trim();
        return new AccessAttempt(enteredIdentifier, connection, now);
    }

    public static PlayerSession ToSession(LoggedInPlayer player)
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

        var fault = new InvalidCredentialsFault();
        var reason = new FaultReason(rejection.Message);
        return new FaultException<InvalidCredentialsFault>(fault, reason);
    }

    public static FaultException<AccountRestrictedFault> ToFault(AccountRestrictedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new AccountRestrictedFault
        {
            Reason = ToReason(rejection.Restriction),
            LockMinutes = rejection.LockMinutes,
        };
        var reason = new FaultReason(rejection.Message);
        return new FaultException<AccountRestrictedFault>(fault, reason);
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
            case AccountRestriction.Banned:
                return RestrictionReason.Banned;
            default:
                throw new ArgumentOutOfRangeException(nameof(restriction), restriction, "Unsupported account restriction.");
        }
    }
}
