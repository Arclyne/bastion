using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Matches;

namespace Bastion.Client.Networking.Matches;

public sealed class MatchClient : IMatchClient
{
    private readonly IServiceChannel<IMatchService> _channel;
    private readonly INotificationClient _notificationClient;

    public MatchClient(IServiceChannel<IMatchService> channel, INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(notificationClient);

        _channel = channel;
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

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<MatchNotJoinedFault>)
        {
            return new MatchNotJoinedException(ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }
}
