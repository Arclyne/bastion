using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Chat;
using Bastion.Server.Host.Matchmaking;
using CoreWCF;
using log4net;

namespace Bastion.Server.Host.Sessions;

public sealed class NotificationChannelRegistry : IChatBroadcaster, IMatchmakingNotifier
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationChannelRegistry));

    private readonly ConcurrentDictionary<int, INotificationCallback> _callbacksByAccountId =
        new ConcurrentDictionary<int, INotificationCallback>();
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly TimeProvider _timeProvider;

    public NotificationChannelRegistry(IMatchmakingQueue matchmakingQueue, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _matchmakingQueue = matchmakingQueue;
        _timeProvider = timeProvider;
    }

    public void RegisterChannel(ConnectedPlayer player, INotificationCallback callback)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(callback);

        _callbacksByAccountId[player.AccountId] = callback;
        var callbackChannel = (ICommunicationObject)callback;
        callbackChannel.Closed += (_, _) => UnregisterChannel(player.AccountId, callback);
        callbackChannel.Faulted += (_, _) => UnregisterChannel(player.AccountId, callback);
        _logger.Info($"Player subscribed to notifications. AccountId={player.AccountId}");
    }

    public void Broadcast(ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ChatMessageNotification notification = ChatTranslator.ToNotification(line);
        foreach (KeyValuePair<int, INotificationCallback> subscription in _callbacksByAccountId)
        {
            DeliverNotification(subscription.Key, subscription.Value, notification);
        }
    }

    public bool HasNotificationChannel(int accountId)
    {
        return _callbacksByAccountId.ContainsKey(accountId);
    }

    public void NotifyMatchFound(CreatedMatch createdMatch)
    {
        ArgumentNullException.ThrowIfNull(createdMatch);

        MatchFoundNotification notification = MatchmakingTranslator.ToNotification(createdMatch, GetUtcNow());
        foreach (CreatedMatchParticipant participant in createdMatch.Participants)
        {
            NotifyPlayer(participant.Player.AccountId, notification);
        }
    }

    public void NotifyPresenceRequested(PresenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        NotifyPlayer(request.AccountId, MatchmakingTranslator.ToNotification(request, GetUtcNow()));
    }

    public void NotifySearchEnded(int accountId, SearchEndReason reason)
    {
        NotifyPlayer(accountId, MatchmakingTranslator.ToNotification(reason, GetUtcNow()));
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    private void NotifyPlayer(int accountId, Notification notification)
    {
        if (!_callbacksByAccountId.TryGetValue(accountId, out INotificationCallback? callback))
        {
            return;
        }

        DeliverNotification(accountId, callback, notification);
    }

    private void DeliverNotification(int accountId, INotificationCallback callback, Notification notification)
    {
        try
        {
            callback.ReceiveNotification(notification);
        }
        catch (CommunicationException ex)
        {
            AbortFailedChannel(accountId, callback, ex);
        }
        catch (TimeoutException ex)
        {
            AbortFailedChannel(accountId, callback, ex);
        }
    }

    private void AbortFailedChannel(int accountId, INotificationCallback callback, Exception cause)
    {
        _logger.Warn($"A notification could not be delivered. AccountId={accountId}", cause);
        ((ICommunicationObject)callback).Abort();
        UnregisterChannel(accountId, callback);
    }

    private void UnregisterChannel(int accountId, INotificationCallback callback)
    {
        if (!_callbacksByAccountId.TryRemove(new KeyValuePair<int, INotificationCallback>(accountId, callback)))
        {
            return;
        }

        _logger.Info($"Player left notifications. AccountId={accountId}");

        // A player without a notification channel would never hear about the match (CU-09 EX-04).
        if (_matchmakingQueue.TryRemove(accountId))
        {
            _logger.Info($"Match search ended because the player disconnected. AccountId={accountId}");
        }
    }
}
