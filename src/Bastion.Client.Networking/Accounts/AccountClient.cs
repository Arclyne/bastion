using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts;
using Bastion.Contracts.Accounts;

namespace Bastion.Client.Networking.Accounts;

public sealed class AccountClient : IAccountClient, IDisposable
{
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
            throw new ServerUnreachableException(ex);
        }
        catch (FaultException<InvalidCredentialsFault> ex)
        {
            throw new InvalidCredentialsException(ex);
        }
        catch (FaultException<AccountRestrictedFault> ex)
        {
            throw new AccountRestrictedException(ToRestriction(ex.Detail.Reason), ex.Detail.LockMinutes, ex);
        }
        catch (FaultException ex)
        {
            throw new ServerErrorException(ex);
        }
        catch (CommunicationException ex)
        {
            throw new ConnectionLostException(ex);
        }
        catch (TimeoutException ex)
        {
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
            case RestrictionReason.Banned:
                return AccountRestriction.Banned;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported restriction reason.");
        }
    }
}
