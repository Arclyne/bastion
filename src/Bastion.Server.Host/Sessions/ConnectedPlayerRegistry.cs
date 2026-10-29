using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CoreWCF;
using log4net;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Chat;

namespace Bastion.Server.Host.Sessions;

public sealed class ConnectedPlayerRegistry : IChatBroadcaster
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ConnectedPlayerRegistry));

    private readonly ConcurrentDictionary<int, INotificationCallback> _callbacks =
        new ConcurrentDictionary<int, INotificationCallback>();

    public void Register(ConnectedPlayer player, INotificationCallback callback)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(callback);

        _callbacks[player.AccountId] = callback;
        var channel = (ICommunicationObject)callback;
        channel.Closed += (sender, args) => Remove(player.AccountId, callback);
        channel.Faulted += (sender, args) => Remove(player.AccountId, callback);
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
        if (_callbacks.TryRemove(new KeyValuePair<int, INotificationCallback>(accountId, callback)))
        {
            _logger.Info($"Player left notifications. AccountId={accountId}");
        }
    }
}
