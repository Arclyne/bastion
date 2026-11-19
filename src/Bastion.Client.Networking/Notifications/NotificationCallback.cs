using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Rooms;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;
using log4net;

namespace Bastion.Client.Networking.Notifications;

/// <inheritdoc/>
[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class NotificationCallback : INotificationCallback
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationCallback));

    private readonly IReadOnlyDictionary<Type, Action<Notification>> _dispatchersByNotificationType;

    public NotificationCallback()
    {
        var dispatchersByNotificationType = new Dictionary<Type, Action<Notification>>();
        AddDispatcher<ChatMessageNotification>(dispatchersByNotificationType, DispatchChatMessage);
        AddDispatcher<MatchChatMessageNotification>(dispatchersByNotificationType, DispatchMatchChatMessage);
        AddDispatcher<ParticipationEndedNotification>(dispatchersByNotificationType, DispatchParticipationEnded);
        AddDispatcher<MatchEndedNotification>(dispatchersByNotificationType, DispatchMatchEnded);
        AddDispatcher<PawnMovedNotification>(dispatchersByNotificationType, DispatchPawnMoved);
        AddDispatcher<WallPlacedNotification>(dispatchersByNotificationType, DispatchWallPlaced);
        AddDispatcher<FriendPresenceNotification>(dispatchersByNotificationType, DispatchFriendPresence);
        AddDispatcher<FriendRequestNotification>(dispatchersByNotificationType, DispatchFriendRequest);
        AddDispatcher<FriendshipStartedNotification>(dispatchersByNotificationType, DispatchFriendshipStarted);
        AddDispatcher<PrivateMessageNotification>(dispatchersByNotificationType, DispatchPrivateMessage);
        AddDispatcher<RoomUpdatedNotification>(dispatchersByNotificationType, DispatchRoomUpdated);
        AddDispatcher<RoomClosedNotification>(dispatchersByNotificationType, DispatchRoomClosed);
        AddDispatcher<MatchFoundNotification>(dispatchersByNotificationType, DispatchMatchFound);
        AddDispatcher<PresenceRequestedNotification>(dispatchersByNotificationType, DispatchPresenceRequested);
        AddDispatcher<SearchEndedNotification>(dispatchersByNotificationType, DispatchSearchEnded);
        AddDispatcher<OpponentDisconnectedNotification>(dispatchersByNotificationType, DispatchOpponentDisconnected);
        AddDispatcher<OpponentReconnectedNotification>(dispatchersByNotificationType, DispatchOpponentReconnected);
        AddDispatcher<MatchTakenOverNotification>(dispatchersByNotificationType, DispatchMatchTakenOver);
        AddDispatcher<DrawOfferedNotification>(dispatchersByNotificationType, DispatchDrawOffered);
        AddDispatcher<DrawRejectedNotification>(dispatchersByNotificationType, DispatchDrawRejected);
        AddDispatcher<DrawOfferExpiredNotification>(dispatchersByNotificationType, DispatchDrawOfferExpired);
        _dispatchersByNotificationType = dispatchersByNotificationType;
    }

    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<ParticipationEndedEventArgs>? ParticipationEnded;

    public event EventHandler<MatchEndedEventArgs>? MatchEnded;

    public event EventHandler<PawnMovedEventArgs>? PawnMoved;

    public event EventHandler<WallPlacedEventArgs>? WallPlaced;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged;

    public event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived;

    public event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted;

    public event EventHandler<PrivateMessageReceivedEventArgs>? PrivateMessageReceived;

    public event EventHandler<RoomUpdatedEventArgs>? RoomUpdated;

    public event EventHandler<RoomClosedEventArgs>? RoomClosed;

    public event EventHandler<OpponentConnectionEventArgs>? OpponentDisconnected;

    public event EventHandler<OpponentConnectionEventArgs>? OpponentReconnected;

    public event EventHandler? MatchTakenOver;

    public event EventHandler<DrawEventArgs>? DrawOffered;

    public event EventHandler<DrawEventArgs>? DrawRejected;

    public event EventHandler<DrawEventArgs>? DrawOfferExpired;

    /// <inheritdoc/>
    public void ReceiveNotification(Notification? notification)
    {
        if (notification is null)
        {
            _logger.Warn("A notification arrived without content.");
            return;
        }

        Type notificationType = notification.GetType();
        Action<Notification>? dispatch = _dispatchersByNotificationType.GetValueOrDefault(notificationType);
        if (dispatch is null)
        {
            _logger.Warn($"A notification of an unknown type was ignored. Type={notificationType.Name}");
            return;
        }

        try
        {
            dispatch(notification);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.Warn(
                $"A notification was ignored because one of its values is out of range. Type={notificationType.Name}",
                ex);
        }
    }

    private static void AddDispatcher<TNotification>(
        Dictionary<Type, Action<Notification>> dispatchersByNotificationType,
        Action<TNotification> dispatch)
        where TNotification : Notification
    {
        dispatchersByNotificationType.Add(typeof(TNotification), notification => dispatch((TNotification)notification));
    }

    private void DispatchChatMessage(ChatMessageNotification chatMessage)
    {
        ChatMessageReceivedEventArgs lobbyMessage = ToChatMessageReceivedEventArgs(chatMessage);
        OnChatMessageReceived(lobbyMessage);
    }

    private static ChatMessageReceivedEventArgs ToChatMessageReceivedEventArgs(ChatMessageNotification notification)
    {
        var message = ChatContractTranslator.ToChatMessage(notification);
        return new ChatMessageReceivedEventArgs(message);
    }

    private void OnChatMessageReceived(ChatMessageReceivedEventArgs e)
    {
        ChatMessageReceived?.Invoke(this, e);
    }

    private void DispatchMatchChatMessage(MatchChatMessageNotification matchChatMessage)
    {
        MatchChatMessageReceivedEventArgs matchMessage = ToMatchChatMessageReceivedEventArgs(matchChatMessage);
        OnMatchChatMessageReceived(matchMessage);
    }

    private static MatchChatMessageReceivedEventArgs ToMatchChatMessageReceivedEventArgs(
        MatchChatMessageNotification notification)
    {
        var message = ChatContractTranslator.ToChatMessage(notification);
        return new MatchChatMessageReceivedEventArgs(notification.MatchId, message);
    }

    private void OnMatchChatMessageReceived(MatchChatMessageReceivedEventArgs e)
    {
        MatchChatMessageReceived?.Invoke(this, e);
    }

    private void DispatchParticipationEnded(ParticipationEndedNotification participationEnded)
    {
        var participationEnd =
            MatchContractTranslator.ToParticipationEndedEventArgs(participationEnded);
        OnParticipationEnded(participationEnd);
    }

    private void OnParticipationEnded(ParticipationEndedEventArgs e)
    {
        ParticipationEnded?.Invoke(this, e);
    }

    private void DispatchMatchEnded(MatchEndedNotification matchEnded)
    {
        var matchEnd = MatchContractTranslator.ToMatchEndedEventArgs(matchEnded);
        OnMatchEnded(matchEnd);
    }

    private void OnMatchEnded(MatchEndedEventArgs e)
    {
        MatchEnded?.Invoke(this, e);
    }

    private void DispatchPawnMoved(PawnMovedNotification pawnMoved)
    {
        PawnMovedEventArgs pawnMove = ToPawnMovedEventArgs(pawnMoved);
        OnPawnMoved(pawnMove);
    }

    private static PawnMovedEventArgs ToPawnMovedEventArgs(PawnMovedNotification notification)
    {
        string moverNickname = notification.MoverNickname ?? string.Empty;
        string toSquare = notification.ToSquare ?? string.Empty;
        return new PawnMovedEventArgs
        {
            MatchId = notification.MatchId,
            MoverNickname = moverNickname,
            ToSquare = toSquare,
            NextTurnOrder = notification.NextTurnOrder,
            NextMoveNumber = notification.NextMoveNumber,
            RemainingClockMilliseconds = notification.RemainingClockMilliseconds,
        };
    }

    private void OnPawnMoved(PawnMovedEventArgs e)
    {
        PawnMoved?.Invoke(this, e);
    }

    private void DispatchWallPlaced(WallPlacedNotification wallPlaced)
    {
        WallPlacedEventArgs placement = ToWallPlacedEventArgs(wallPlaced);
        OnWallPlaced(placement);
    }

    private static WallPlacedEventArgs ToWallPlacedEventArgs(WallPlacedNotification notification)
    {
        string placerNickname = notification.PlacerNickname ?? string.Empty;
        string groove = notification.Groove ?? string.Empty;
        PlacedWallOrientation orientation = MatchContractTranslator.ToPlacedOrientation(notification.Orientation);
        return new WallPlacedEventArgs
        {
            MatchId = notification.MatchId,
            PlacerNickname = placerNickname,
            Groove = groove,
            Orientation = orientation,
            RemainingWalls = notification.RemainingWalls,
            NextTurnOrder = notification.NextTurnOrder,
            NextMoveNumber = notification.NextMoveNumber,
            RemainingClockMilliseconds = notification.RemainingClockMilliseconds,
        };
    }

    private void OnWallPlaced(WallPlacedEventArgs e)
    {
        WallPlaced?.Invoke(this, e);
    }

    private void DispatchFriendPresence(FriendPresenceNotification friendPresence)
    {
        FriendPresenceChangedEventArgs presenceChange = ToFriendPresenceChangedEventArgs(friendPresence);
        OnFriendPresenceChanged(presenceChange);
    }

    private static FriendPresenceChangedEventArgs ToFriendPresenceChangedEventArgs(
        FriendPresenceNotification notification)
    {
        var friend = FriendContractTranslator.ToListedFriend(notification);
        return new FriendPresenceChangedEventArgs(friend, notification.OccurredAt);
    }

    private void OnFriendPresenceChanged(FriendPresenceChangedEventArgs e)
    {
        FriendPresenceChanged?.Invoke(this, e);
    }

    private void DispatchFriendRequest(FriendRequestNotification friendRequest)
    {
        FriendRequestReceivedEventArgs request = ToFriendRequestReceivedEventArgs(friendRequest);
        OnFriendRequestReceived(request);
    }

    private static FriendRequestReceivedEventArgs ToFriendRequestReceivedEventArgs(
        FriendRequestNotification notification)
    {
        PendingFriendRequest request = FriendContractTranslator.ToPendingRequest(notification);
        return new FriendRequestReceivedEventArgs(request);
    }

    private void OnFriendRequestReceived(FriendRequestReceivedEventArgs e)
    {
        FriendRequestReceived?.Invoke(this, e);
    }

    private void DispatchFriendshipStarted(FriendshipStartedNotification friendshipStarted)
    {
        FriendshipStartedEventArgs friendship = ToFriendshipStartedEventArgs(friendshipStarted);
        OnFriendshipStarted(friendship);
    }

    private static FriendshipStartedEventArgs ToFriendshipStartedEventArgs(FriendshipStartedNotification notification)
    {
        var friend = FriendContractTranslator.ToPlayerTag(notification);
        return new FriendshipStartedEventArgs(friend);
    }

    private void OnFriendshipStarted(FriendshipStartedEventArgs e)
    {
        FriendshipStarted?.Invoke(this, e);
    }

    private void DispatchPrivateMessage(PrivateMessageNotification privateMessage)
    {
        PrivateMessageReceivedEventArgs message = ToPrivateMessageReceivedEventArgs(privateMessage);
        OnPrivateMessageReceived(message);
    }

    private static PrivateMessageReceivedEventArgs ToPrivateMessageReceivedEventArgs(
        PrivateMessageNotification notification)
    {
        PlayerTag sender = ConversationContractTranslator.ToSender(notification);
        var message = ConversationContractTranslator.ToPrivateMessage(notification);
        return new PrivateMessageReceivedEventArgs(sender, message);
    }

    private void OnPrivateMessageReceived(PrivateMessageReceivedEventArgs e)
    {
        PrivateMessageReceived?.Invoke(this, e);
    }

    private void DispatchRoomUpdated(RoomUpdatedNotification roomUpdated)
    {
        if (roomUpdated.Room is null)
        {
            _logger.Warn("A room update arrived without the room and was ignored.");
            return;
        }

        RoomUpdatedEventArgs update = ToRoomUpdatedEventArgs(roomUpdated.Room);
        OnRoomUpdated(update);
    }

    private static RoomUpdatedEventArgs ToRoomUpdatedEventArgs(RoomState room)
    {
        var privateRoom = RoomContractTranslator.ToPrivateRoom(room);
        return new RoomUpdatedEventArgs(privateRoom);
    }

    private void OnRoomUpdated(RoomUpdatedEventArgs e)
    {
        RoomUpdated?.Invoke(this, e);
    }

    private void DispatchRoomClosed(RoomClosedNotification roomClosed)
    {
        RoomClosedEventArgs closure = ToRoomClosedEventArgs(roomClosed);
        OnRoomClosed(closure);
    }

    private static RoomClosedEventArgs ToRoomClosedEventArgs(RoomClosedNotification notification)
    {
        RoomCloseReason reason = RoomContractTranslator.ToCloseReason(notification.Reason);
        string code = notification.Code ?? string.Empty;
        return new RoomClosedEventArgs(code, reason);
    }

    private void OnRoomClosed(RoomClosedEventArgs e)
    {
        RoomClosed?.Invoke(this, e);
    }

    private void DispatchMatchFound(MatchFoundNotification matchFound)
    {
        MatchFoundEventArgs foundMatch = ToMatchFoundEventArgs(matchFound);
        OnMatchFound(foundMatch);
    }

    private static MatchFoundEventArgs ToMatchFoundEventArgs(MatchFoundNotification notification)
    {
        MatchFoundParticipant[] notifiedParticipants =
            notification.Participants ?? Array.Empty<MatchFoundParticipant>();
        IEnumerable<FoundMatchParticipant> foundParticipants = notifiedParticipants.Select(ToFoundMatchParticipant);
        List<FoundMatchParticipant> participants = [.. foundParticipants];
        var format = new FoundMatchFormat(
            notification.BoardSize,
            notification.WallsPerPlayer,
            notification.ClockMinutes);
        return new MatchFoundEventArgs(notification.MatchId, format, participants);
    }

    private static FoundMatchParticipant ToFoundMatchParticipant(MatchFoundParticipant participant)
    {
        string nickname = participant.Nickname ?? string.Empty;
        return new FoundMatchParticipant(nickname, participant.EloRating, participant.TurnOrder);
    }

    private void OnMatchFound(MatchFoundEventArgs e)
    {
        MatchFound?.Invoke(this, e);
    }

    private void DispatchPresenceRequested(PresenceRequestedNotification presenceRequested)
    {
        PresenceRequestedEventArgs presenceRequest = ToPresenceRequestedEventArgs(presenceRequested);
        OnPresenceRequested(presenceRequest);
    }

    private static PresenceRequestedEventArgs ToPresenceRequestedEventArgs(PresenceRequestedNotification notification)
    {
        TimeSpan responseWindow = notification.Deadline - notification.OccurredAt;
        return new PresenceRequestedEventArgs(responseWindow);
    }

    private void OnPresenceRequested(PresenceRequestedEventArgs e)
    {
        PresenceRequested?.Invoke(this, e);
    }

    private void DispatchSearchEnded(SearchEndedNotification searchEnded)
    {
        SearchEndedEventArgs searchEnd = ToSearchEndedEventArgs(searchEnded);
        OnSearchEnded(searchEnd);
    }

    private static SearchEndedEventArgs ToSearchEndedEventArgs(SearchEndedNotification notification)
    {
        SearchEndReason reason = ToReason(notification.Reason);
        return new SearchEndedEventArgs(reason);
    }

    private static SearchEndReason ToReason(SearchEndedReason reason)
    {
        switch (reason)
        {
            case SearchEndedReason.Cancelled:
                return SearchEndReason.Cancelled;
            case SearchEndedReason.PresenceExpired:
                return SearchEndReason.PresenceExpired;
            case SearchEndedReason.MatchCreationFailed:
                return SearchEndReason.MatchCreationFailed;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported search end reason.");
        }
    }

    private void OnSearchEnded(SearchEndedEventArgs e)
    {
        SearchEnded?.Invoke(this, e);
    }

    private void DispatchOpponentDisconnected(OpponentDisconnectedNotification disconnected)
    {
        var connection = new OpponentConnectionEventArgs
        {
            MatchId = disconnected.MatchId,
            TurnOrder = disconnected.TurnOrder,
            OccurredAt = disconnected.OccurredAt,
        };
        OpponentDisconnected?.Invoke(this, connection);
    }

    private void DispatchOpponentReconnected(OpponentReconnectedNotification reconnected)
    {
        var connection = new OpponentConnectionEventArgs
        {
            MatchId = reconnected.MatchId,
            TurnOrder = reconnected.TurnOrder,
            OccurredAt = reconnected.OccurredAt,
        };
        OpponentReconnected?.Invoke(this, connection);
    }

    private void DispatchMatchTakenOver(MatchTakenOverNotification takenOver)
    {
        _logger.Warn($"The match continues on another device. OccurredAt={takenOver.OccurredAt:O}");
        MatchTakenOver?.Invoke(this, EventArgs.Empty);
    }

    private void DispatchDrawOffered(DrawOfferedNotification drawOffered)
    {
        var draw = new DrawEventArgs
        {
            MatchId = drawOffered.MatchId,
            OffererTurnOrder = drawOffered.OffererTurnOrder,
        };
        DrawOffered?.Invoke(this, draw);
    }

    private void DispatchDrawRejected(DrawRejectedNotification drawRejected)
    {
        var draw = new DrawEventArgs
        {
            MatchId = drawRejected.MatchId,
            OffererTurnOrder = 0,
        };
        DrawRejected?.Invoke(this, draw);
    }

    private void DispatchDrawOfferExpired(DrawOfferExpiredNotification drawOfferExpired)
    {
        var draw = new DrawEventArgs
        {
            MatchId = drawOfferExpired.MatchId,
            OffererTurnOrder = 0,
        };
        DrawOfferExpired?.Invoke(this, draw);
    }
}
