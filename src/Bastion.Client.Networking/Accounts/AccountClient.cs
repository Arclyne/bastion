using System;
using System.ServiceModel;
using log4net;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts;
using Bastion.Contracts.Accounts;

namespace Bastion.Client.Networking.Accounts;

public sealed class AccountClient : IAccountClient, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountClient));

    private readonly ServiceChannel<IAccountService> _channel = new ServiceChannel<IAccountService>(ServiceEndpoints.AccountsPath);

    public SignedInSession LogIn(LoginCredentials credentials)
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
        try
        {
            PlayerSession session = _channel.Call(service => service.LogIn(playerCredentials, device));
            return ToSignedInSession(session);
        }
        catch (EndpointNotFoundException ex)
        {
            _logger.Warn("The server could not be reached.", ex);
            throw new ServerUnreachableException(ex);
        }
        catch (FaultException<InvalidCredentialsFault> ex)
        {
            _logger.Warn("The server turned down the credentials.", ex);
            throw new InvalidCredentialsException(ex);
        }
        catch (FaultException<AccountRestrictedFault> ex)
        {
            _logger.Warn("The server turned down a restricted account.", ex);
            throw new AccountRestrictedException(ToRestriction(ex.Detail.Reason), ex.Detail.LockMinutes, ex);
        }
        catch (FaultException ex)
        {
            _logger.Warn("The server could not complete the sign-in.", ex);
            throw new ServerErrorException(ex);
        }
        catch (CommunicationException ex)
        {
            _logger.Warn("The connection with the server was lost.", ex);
            throw new ConnectionLostException(ex);
        }
        catch (TimeoutException ex)
        {
            _logger.Warn("The server did not answer in time.", ex);
            throw new ConnectionLostException(ex);
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    private static SignedInSession ToSignedInSession(PlayerSession session)
    {
        return new SignedInSession(
            session.Token ?? string.Empty,
            session.Nickname ?? string.Empty,
            session.PreferredLanguage ?? string.Empty);
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
            default:
                return AccountRestriction.Banned;
        }
    }
}
