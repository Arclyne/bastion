using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Matchmaking;

namespace Bastion.Client.Networking.Matchmaking;

public sealed class MatchmakingClient : IMatchmakingClient
{
    private readonly IServiceChannel<IMatchmakingService> _channel;
    private readonly INotificationClient _notificationClient;

    public MatchmakingClient(IServiceChannel<IMatchmakingService> channel, INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(notificationClient);

        _channel = channel;
        _notificationClient = notificationClient;
    }

    public event EventHandler<MatchFoundEventArgs>? MatchFound
    {
        add => _notificationClient.MatchFound += value;
        remove => _notificationClient.MatchFound -= value;
    }

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested
    {
        add => _notificationClient.PresenceRequested += value;
        remove => _notificationClient.PresenceRequested -= value;
    }

    public event EventHandler<SearchEndedEventArgs>? SearchEnded
    {
        add => _notificationClient.SearchEnded += value;
        remove => _notificationClient.SearchEnded -= value;
    }

    public event EventHandler? Disconnected
    {
        add => _notificationClient.Disconnected += value;
        remove => _notificationClient.Disconnected -= value;
    }

    public void StartSearch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Call(service => service.StartSearch(sessionToken));
    }

    public void CancelSearch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Call(service => service.CancelSearch(sessionToken));
    }

    public void ConfirmPresence(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Call(service => service.ConfirmPresence(sessionToken));
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<SearchRejectedFault> rejected)
        {
            return new SearchRejectedException(ToRejection(rejected.Detail.Reason), ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private static SearchRejection ToRejection(SearchRejectionReason reason)
    {
        switch (reason)
        {
            case SearchRejectionReason.NotificationsUnavailable:
                return SearchRejection.NotificationsUnavailable;
            case SearchRejectionReason.UnfinishedMatch:
                return SearchRejection.UnfinishedMatch;
            case SearchRejectionReason.InRoom:
                return SearchRejection.InRoom;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported search rejection reason.");
        }
    }

    private void Call(Action<IMatchmakingService> operation)
    {
        try
        {
            _channel.Call(operation);
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
}
