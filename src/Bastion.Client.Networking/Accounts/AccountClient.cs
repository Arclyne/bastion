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

    private readonly ServiceChannel<IAccountService> _channel =
        new ServiceChannel<IAccountService>(ServiceEndpoints.AccountsPath);

    public LoginOutcome LogIn(LoginCredentials credentials)
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
            return new LoginOutcome(LoginStatus.Success, ToSignedInSession(session), 0);
        }
        catch (EndpointNotFoundException ex)
        {
            _logger.Warn("The server could not be reached.", ex);
            return LoginOutcome.Failed(LoginStatus.ServerUnreachable);
        }
        catch (FaultException ex)
        {
            _logger.Warn("The server turned down the sign-in.", ex);
            return ToOutcome(ex);
        }
        catch (CommunicationException ex)
        {
            _logger.Warn("The connection with the server was lost.", ex);
            return LoginOutcome.Failed(LoginStatus.ConnectionLost);
        }
        catch (TimeoutException ex)
        {
            _logger.Warn("The server did not answer in time.", ex);
            return LoginOutcome.Failed(LoginStatus.ConnectionLost);
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

    private static LoginOutcome ToOutcome(FaultException fault)
    {
        switch (fault)
        {
            case FaultException<InvalidCredentialsFault>:
                return LoginOutcome.Failed(LoginStatus.InvalidCredentials);
            case FaultException<AccountRestrictedFault> restricted:
                return new LoginOutcome(ToStatus(restricted.Detail.Reason), null, restricted.Detail.LockMinutes);
            default:
                return LoginOutcome.Failed(LoginStatus.ServerError);
        }
    }

    private static LoginStatus ToStatus(RestrictionReason reason)
    {
        switch (reason)
        {
            case RestrictionReason.Locked:
                return LoginStatus.AccountLocked;
            case RestrictionReason.Pending:
                return LoginStatus.AccountPending;
            case RestrictionReason.Suspended:
                return LoginStatus.AccountSuspended;
            default:
                return LoginStatus.AccountBanned;
        }
    }
}
