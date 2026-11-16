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

        Action<IMatchmakingService> startSearch = service => service.StartSearch(sessionToken);
        Call(startSearch);
    }

    public void CancelSearch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Action<IMatchmakingService> cancelSearch = service => service.CancelSearch(sessionToken);
        Call(cancelSearch);
    }

    public void ConfirmPresence(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Action<IMatchmakingService> confirmPresence = service => service.ConfirmPresence(sessionToken);
        Call(confirmPresence);
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

    private static Exception ToClientException(Exception cause)
    {
        if (cause is FaultException<SearchRejectedFault> rejected)
        {
            SearchRejection rejection = ToRejection(rejected.Detail.Reason);
            return new SearchRejectedException(rejection, cause);
        }

        return ServiceFailureTranslator.ToClientException(cause);
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
}
