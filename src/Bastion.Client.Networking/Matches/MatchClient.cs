using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts;
using Bastion.Contracts.Matches;

namespace Bastion.Client.Networking.Matches;

public sealed class MatchClient : IMatchClient, IDisposable
{
    private readonly ServiceChannel<IMatchService> _channel =
        new ServiceChannel<IMatchService>(ServiceEndpoints.MatchesPath);
    private readonly INotificationClient _notificationClient;

    public MatchClient(INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(notificationClient);

        _notificationClient = notificationClient;
    }

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned
    {
        add => _notificationClient.MatchAbandoned += value;
        remove => _notificationClient.MatchAbandoned -= value;
    }

    public void AbandonMatch(string sessionToken, int matchId)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        try
        {
            _channel.Call(service => service.AbandonMatch(sessionToken, matchId));
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

    public void Dispose()
    {
        _channel.Dispose();
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<MatchNotJoinedFault>)
        {
            return new MatchNotJoinedException(ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }
}
