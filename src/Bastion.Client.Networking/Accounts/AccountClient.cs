using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts.Accounts;

namespace Bastion.Client.Networking.Accounts;

public sealed class AccountClient : IAccountClient
{
    private readonly IServiceChannel<IAccountService> _channel;

    public AccountClient(IServiceChannel<IAccountService> channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        _channel = channel;
    }

    public LoggedInSession LogIn(LoginCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        var playerCredentials = new PlayerCredentials
        {
            Identifier = credentials.Identifier,
            Password = credentials.Password,
        };
        var device = new ClientDevice
        {
            DeviceFingerprint = DeviceIdentity.GetFingerprint(),
            ClientVersion = DeviceIdentity.GetClientVersion(),
        };
        PlayerSession session = Call(service => service.LogIn(playerCredentials, device));
        return ToLoggedInSession(session);
    }

    private TResult Call<TResult>(Func<IAccountService, TResult> operation)
    {
        try
        {
            return _channel.Call(operation);
        }
        catch (CommunicationException ex)
        {
            throw ToClientException(ex);
        }
        catch (TimeoutException ex)
        {
            throw ToClientException(ex);
        }
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<InvalidCredentialsFault>)
        {
            return new InvalidCredentialsException(ex);
        }

        if (ex is FaultException<AccountRestrictedFault> restricted)
        {
            AccountRestriction restriction = ToRestriction(restricted.Detail.Reason);
            return new AccountRestrictedException(restriction, restricted.Detail.LockMinutes, ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private static AccountRestriction ToRestriction(RestrictionReason reason)
    {
        switch (reason)
        {
            case RestrictionReason.Locked:
                return AccountRestriction.Locked;
            case RestrictionReason.Pending:
                return AccountRestriction.Pending;
            case RestrictionReason.Suspended:
                return AccountRestriction.Suspended;
            case RestrictionReason.Banned:
                return AccountRestriction.Banned;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported restriction reason.");
        }
    }

    private static LoggedInSession ToLoggedInSession(PlayerSession session)
    {
        string token = session.Token ?? string.Empty;
        string nickname = session.Nickname ?? string.Empty;
        string preferredLanguage = session.PreferredLanguage ?? string.Empty;
        return new LoggedInSession(token, nickname, preferredLanguage);
    }
}
