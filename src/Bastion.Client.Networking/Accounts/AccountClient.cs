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
            AccessDecision decision = _channel.Call(service => service.LogIn(playerCredentials, device));
            return ToOutcome(decision);
        }
        catch (EndpointNotFoundException ex)
        {
            _logger.Warn("The server could not be reached.", ex);
            return LoginOutcome.Failed(LoginStatus.ServerUnreachable);
        }
        catch (FaultException ex)
        {
            _logger.Warn("The server answered with a fault.", ex);
            return LoginOutcome.Failed(LoginStatus.ServerError);
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

    private static LoginOutcome ToOutcome(AccessDecision decision)
    {
        if (decision.Code != AccessDecisionCode.Granted || decision.Session is null)
        {
            return new LoginOutcome(ToStatus(decision.Code), null, decision.LockMinutes);
        }

        var session = new SignedInSession(
            decision.Session.Token ?? string.Empty,
            decision.Session.Nickname ?? string.Empty,
            decision.Session.PreferredLanguage ?? string.Empty);
        return new LoginOutcome(LoginStatus.Success, session, 0);
    }

    private static LoginStatus ToStatus(AccessDecisionCode code)
    {
        switch (code)
        {
            case AccessDecisionCode.InvalidCredentials:
                return LoginStatus.InvalidCredentials;
            case AccessDecisionCode.AccountLocked:
                return LoginStatus.AccountLocked;
            case AccessDecisionCode.AccountPending:
                return LoginStatus.AccountPending;
            case AccessDecisionCode.AccountSuspended:
                return LoginStatus.AccountSuspended;
            case AccessDecisionCode.AccountBanned:
                return LoginStatus.AccountBanned;
            default:
                return LoginStatus.ServerError;
        }
    }
}
