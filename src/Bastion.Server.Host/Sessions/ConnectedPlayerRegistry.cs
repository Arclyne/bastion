using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Bastion.Contracts.Notifications;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Chat;
using Bastion.Server.Host.Matchmaking;
using CoreWCF;
using log4net;

namespace Bastion.Server.Host.Sessions;

public sealed class ConnectedPlayerRegistry : IChatBroadcaster, IMatchmakingNotifier
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ConnectedPlayerRegistry));

    private readonly ConcurrentDictionary<int, INotificationCallback> _callbacks =
        new ConcurrentDictionary<int, INotificationCallback>();
    private readonly IMatchmakingQueue _queue;
    private readonly TimeProvider _timeProvider;

    public ConnectedPlayerRegistry(IMatchmakingQueue queue, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _queue = queue;
        _timeProvider = timeProvider;
    }

    public void Register(ConnectedPlayer player, INotificationCallback callback)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(callback);

        _callbacks[player.AccountId] = callback;
        var channel = (ICommunicationObject)callback;
        channel.Closed += (sender, e) => Remove(player.AccountId, callback);
        channel.Faulted += (sender, e) => Remove(player.AccountId, callback);
        _logger.Info($"Player subscribed to notifications. AccountId={player.AccountId}");
    }

    public void Broadcast(ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ChatMessageNotification notification = ChatTranslator.ToNotification(line);
        foreach (KeyValuePair<int, INotificationCallback> entry in _callbacks)
        {
            Notify(entry, notification);
        }
    }

    public bool CanNotify(int accountId)
    {
        return _callbacks.ContainsKey(accountId);
    }

    public void NotifyMatchFound(MatchPairing pairing)
    {
        ArgumentNullException.ThrowIfNull(pairing);

        MatchFoundNotification notification = MatchmakingTranslator.ToNotification(pairing, GetNow());
        foreach (MatchSearch player in pairing.Players)
        {
            NotifyPlayer(player.AccountId, notification);
        }
    }

    public void RequestPresence(PresenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        NotifyPlayer(request.AccountId, MatchmakingTranslator.ToNotification(request, GetNow()));
    }

    public void NotifySearchEnded(int accountId, SearchEndReason reason)
    {
        NotifyPlayer(accountId, MatchmakingTranslator.ToNotification(reason, GetNow()));
    }

    private DateTime GetNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    private void NotifyPlayer(int accountId, Notification notification)
    {
        if (!_callbacks.TryGetValue(accountId, out INotificationCallback? callback))
        {
            return;
        }

        Notify(new KeyValuePair<int, INotificationCallback>(accountId, callback), notification);
    }

    private void Notify(KeyValuePair<int, INotificationCallback> entry, Notification notification)
    {
        try
        {
            entry.Value.ReceiveNotification(notification);
        }
        catch (CommunicationException ex)
        {
            Drop(entry, ex);
        }
        catch (TimeoutException ex)
        {
            Drop(entry, ex);
        }
    }

    private void Drop(KeyValuePair<int, INotificationCallback> entry, Exception cause)
    {
        _logger.Warn($"A notification could not be delivered. AccountId={entry.Key}", cause);
        ((ICommunicationObject)entry.Value).Abort();
        Remove(entry.Key, entry.Value);
    }

    private void Remove(int accountId, INotificationCallback callback)
    {
        if (!_callbacks.TryRemove(new KeyValuePair<int, INotificationCallback>(accountId, callback)))
        {
            return;
        }

        _logger.Info($"Player left notifications. AccountId={accountId}");

        // A player without a notification channel would never hear about the match (CU-09 EX-04).
        if (_queue.TryRemove(accountId))
        {
            _logger.Info($"Match search ended because the player disconnected. AccountId={accountId}");
        }
    }
}
