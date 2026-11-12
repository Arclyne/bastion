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

    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

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

    /// <inheritdoc/>
    public void ReceiveNotification(Notification? notification)
    {
        if (notification is null)
        {
            _logger.Warn("A notification arrived without content.");
            return;
        }

        if (TryDispatchChatNotification(notification)
            || TryDispatchMatchEndNotification(notification)
            || TryDispatchMatchMoveNotification(notification)
            || TryDispatchFriendNotification(notification)
            || TryDispatchPrivateMessageNotification(notification)
            || TryDispatchRoomNotification(notification)
            || TryDispatchMatchmakingNotification(notification))
        {
            return;
        }

        _logger.Warn($"A notification of an unknown type was ignored. Type={notification.GetType().Name}");
    }

    private bool TryDispatchChatNotification(Notification notification)
    {
        if (notification is ChatMessageNotification chatMessage)
        {
            OnChatMessageReceived(ToChatMessageReceivedEventArgs(chatMessage));
            return true;
        }

        if (notification is MatchChatMessageNotification matchChatMessage)
        {
            OnMatchChatMessageReceived(ToMatchChatMessageReceivedEventArgs(matchChatMessage));
            return true;
        }

        return false;
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

    private bool TryDispatchMatchEndNotification(Notification notification)
    {
        if (notification is not MatchAbandonedNotification matchAbandoned)
        {
            return false;
        }

        OnMatchAbandoned(ToMatchAbandonedEventArgs(matchAbandoned));
        return true;
    }

    private static MatchAbandonedEventArgs ToMatchAbandonedEventArgs(MatchAbandonedNotification notification)
    {
        string abandoningNickname = notification.AbandoningNickname ?? string.Empty;
        return new MatchAbandonedEventArgs(notification.MatchId, abandoningNickname, notification.IsMatchOver);
    }

    private void OnMatchAbandoned(MatchAbandonedEventArgs e)
    {
        MatchAbandoned?.Invoke(this, e);
    }

    private bool TryDispatchMatchMoveNotification(Notification notification)
    {
        if (notification is PawnMovedNotification pawnMoved)
        {
            OnPawnMoved(ToPawnMovedEventArgs(pawnMoved));
            return true;
        }

        if (notification is WallPlacedNotification wallPlaced)
        {
            OnWallPlaced(MatchContractTranslator.ToWallPlacedEventArgs(wallPlaced));
            return true;
        }

        return false;
    }

    private static PawnMovedEventArgs ToPawnMovedEventArgs(PawnMovedNotification notification)
    {
        string moverNickname = notification.MoverNickname ?? string.Empty;
        string fromSquare = notification.FromSquare ?? string.Empty;
        string toSquare = notification.ToSquare ?? string.Empty;
        return new PawnMovedEventArgs
        {
            MatchId = notification.MatchId,
            MoverNickname = moverNickname,
            FromSquare = fromSquare,
            ToSquare = toSquare,
            NextTurnOrder = notification.NextTurnOrder,
            NextMoveNumber = notification.NextMoveNumber,
            RemainingClockMilliseconds = notification.RemainingClockMilliseconds,
            IsMatchOver = notification.IsMatchOver,
        };
    }

    private void OnPawnMoved(PawnMovedEventArgs e)
    {
        PawnMoved?.Invoke(this, e);
    }

    private void OnWallPlaced(WallPlacedEventArgs e)
    {
        WallPlaced?.Invoke(this, e);
    }

    private bool TryDispatchFriendNotification(Notification notification)
    {
        if (notification is FriendPresenceNotification friendPresence)
        {
            OnFriendPresenceChanged(ToFriendPresenceChangedEventArgs(friendPresence));
            return true;
        }

        if (notification is FriendRequestNotification friendRequest)
        {
            OnFriendRequestReceived(ToFriendRequestReceivedEventArgs(friendRequest));
            return true;
        }

        if (notification is FriendshipStartedNotification friendshipStarted)
        {
            OnFriendshipStarted(ToFriendshipStartedEventArgs(friendshipStarted));
            return true;
        }

        return false;
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

    private static FriendshipStartedEventArgs ToFriendshipStartedEventArgs(FriendshipStartedNotification notification)
    {
        var friend = FriendContractTranslator.ToPlayerTag(notification);
        return new FriendshipStartedEventArgs(friend);
    }

    private void OnFriendshipStarted(FriendshipStartedEventArgs e)
    {
        FriendshipStarted?.Invoke(this, e);
    }

    private bool TryDispatchPrivateMessageNotification(Notification notification)
    {
        if (notification is not PrivateMessageNotification privateMessageNotification)
        {
            return false;
        }

        OnPrivateMessageReceived(ToPrivateMessageReceivedEventArgs(privateMessageNotification));
        return true;
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

    private bool TryDispatchRoomNotification(Notification notification)
    {
        if (notification is RoomUpdatedNotification { Room: not null } roomUpdated)
        {
            OnRoomUpdated(ToRoomUpdatedEventArgs(roomUpdated.Room));
            return true;
        }

        if (notification is RoomUpdatedNotification)
        {
            _logger.Warn("A room update arrived without the room and was ignored.");
            return true;
        }

        if (notification is RoomClosedNotification roomClosed)
        {
            OnRoomClosed(ToRoomClosedEventArgs(roomClosed));
            return true;
        }

        return false;
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

    private bool TryDispatchMatchmakingNotification(Notification notification)
    {
        if (notification is MatchFoundNotification matchFound)
        {
            OnMatchFound(ToMatchFoundEventArgs(matchFound));
            return true;
        }

        if (notification is PresenceRequestedNotification presenceRequested)
        {
            OnPresenceRequested(ToPresenceRequestedEventArgs(presenceRequested));
            return true;
        }

        if (notification is SearchEndedNotification searchEnded)
        {
            OnSearchEnded(ToSearchEndedEventArgs(searchEnded));
            return true;
        }

        return false;
    }

    private static MatchFoundEventArgs ToMatchFoundEventArgs(MatchFoundNotification notification)
    {
        MatchFoundParticipant[] notifiedParticipants =
            notification.Participants ?? Array.Empty<MatchFoundParticipant>();
        IEnumerable<FoundMatchParticipant> foundParticipants = notifiedParticipants.Select(ToFoundMatchParticipant);
        var participants = new List<FoundMatchParticipant>(foundParticipants);
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

    private static PresenceRequestedEventArgs ToPresenceRequestedEventArgs(PresenceRequestedNotification notification)
    {
        TimeSpan responseWindow = notification.Deadline - notification.OccurredAt;
        return new PresenceRequestedEventArgs(responseWindow);
    }

    private void OnPresenceRequested(PresenceRequestedEventArgs e)
    {
        PresenceRequested?.Invoke(this, e);
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
}
