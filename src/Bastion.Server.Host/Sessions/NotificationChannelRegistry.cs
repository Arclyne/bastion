using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Chat;
using Bastion.Server.Host.Friends;
using Bastion.Server.Host.Matches;
using Bastion.Server.Host.Matchmaking;
using Bastion.Server.Host.PrivateChat;
using Bastion.Server.Host.Rooms;
using CoreWCF;
using log4net;

namespace Bastion.Server.Host.Sessions;

public sealed class NotificationChannelRegistry
    : IChatBroadcaster,
    IMatchChatNotifier,
    IMatchEndNotifier,
    IMatchmakingNotifier,
    IPlayerPresence,
    IFriendNotifier,
    IPrivateMessageNotifier,
    IRoomNotifier
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationChannelRegistry));

    private readonly ConcurrentDictionary<int, INotificationCallback> _callbacksByAccountId =
        new ConcurrentDictionary<int, INotificationCallback>();
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly FriendPresenceReporter _presenceReporter;
    private readonly TimeProvider _timeProvider;

    public NotificationChannelRegistry(
        IMatchmakingQueue matchmakingQueue,
        FriendPresenceReporter presenceReporter,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(presenceReporter);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _matchmakingQueue = matchmakingQueue;
        _presenceReporter = presenceReporter;
        _timeProvider = timeProvider;
    }

    public event EventHandler<PlayerDisconnectedEventArgs>? PlayerDisconnected;

    public void RegisterChannel(ConnectedPlayer player, INotificationCallback callback)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(callback);

        _callbacksByAccountId[player.AccountId] = callback;
        var callbackChannel = (ICommunicationObject)callback;
        callbackChannel.Closed += (_, _) => UnregisterChannel(player.AccountId, callback);
        callbackChannel.Faulted += (_, _) => UnregisterChannel(player.AccountId, callback);
        _logger.Info($"Player subscribed to notifications. AccountId={player.AccountId}");
        _presenceReporter.ReportConnected(player.AccountId);
    }

    public void Broadcast(PostedChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ChatMessageNotification notification = ChatTranslator.ToNotification(line);
        int senderAccountId = line.Line.Sender.AccountId;
        foreach (KeyValuePair<int, INotificationCallback> subscription in _callbacksByAccountId)
        {
            if (subscription.Key != senderAccountId)
            {
                DeliverNotification(subscription.Key, subscription.Value, notification);
            }
        }
    }

    public int CountListeners()
    {
        return _callbacksByAccountId.Count;
    }

    public bool HasNotificationChannel(int accountId)
    {
        return _callbacksByAccountId.ContainsKey(accountId);
    }

    public bool IsPlayerConnected(int accountId)
    {
        return HasNotificationChannel(accountId);
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

    public void NotifyMatchChatMessage(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        PostedChatLine postedLine)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(postedLine);

        MatchChatMessageNotification notification = ChatTranslator.ToMatchChatNotification(matchId, postedLine);
        NotifyPlayers(recipientAccountIds, notification);
    }

    public void NotifyMatchAbandoned(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        AbandonmentNotice notice)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(notice);

        MatchAbandonedNotification notification =
            MatchTranslator.ToAbandonedNotification(matchId, notice, GetUtcNow());
        NotifyPlayers(recipientAccountIds, notification);
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

    public void NotifyPresenceChanged(IReadOnlyList<int> recipientAccountIds, FriendPresenceChange change)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(change);

        FriendPresenceNotification notification = FriendshipTranslator.ToNotification(change, GetUtcNow());
        NotifyPlayers(recipientAccountIds, notification);
    }

    public void NotifyFriendRequestReceived(int recipientAccountId, FriendRequestSummary request)
    {
        ArgumentNullException.ThrowIfNull(request);

        NotifyPlayer(recipientAccountId, FriendshipTranslator.ToRequestNotification(request, GetUtcNow()));
    }

    public void NotifyFriendshipStarted(int accountId, PlayerIdentity friend)
    {
        ArgumentNullException.ThrowIfNull(friend);

        NotifyPlayer(accountId, FriendshipTranslator.ToFriendshipStartedNotification(friend, GetUtcNow()));
    }

    public void NotifyPrivateMessage(PrivateMessageDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        NotifyPlayer(delivery.Message.RecipientId.GetValueOrDefault(), PrivateChatTranslator.ToNotification(delivery));
    }

    public void NotifyRoomUpdated(RoomSnapshot room, int originAccountId)
    {
        ArgumentNullException.ThrowIfNull(room);

        RoomUpdatedNotification notification = RoomTranslator.ToUpdatedNotification(room, GetUtcNow());
        NotifyPlayers(room.ListAccountIdsExcept(originAccountId), notification);
    }

    public void NotifyRoomClosed(RoomSnapshot room, int originAccountId)
    {
        ArgumentNullException.ThrowIfNull(room);

        RoomClosedNotification notification = RoomTranslator.ToClosedNotification(room, GetUtcNow());
        NotifyPlayers(room.ListAccountIdsExcept(originAccountId), notification);
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    private void NotifyPlayers(IEnumerable<int> accountIds, Notification notification)
    {
        foreach (int accountId in accountIds)
        {
            NotifyPlayer(accountId, notification);
        }
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
        _presenceReporter.ReportDisconnected(accountId);

        if (_matchmakingQueue.TryRemove(accountId))
        {
            _logger.Info($"Match search ended because the player disconnected. AccountId={accountId}");
        }

        OnPlayerDisconnected(new PlayerDisconnectedEventArgs(accountId));
    }

    private void OnPlayerDisconnected(PlayerDisconnectedEventArgs e)
    {
        PlayerDisconnected?.Invoke(this, e);
    }
}
