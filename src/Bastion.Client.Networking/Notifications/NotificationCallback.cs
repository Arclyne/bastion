using System;
using System.Linq;
using System.ServiceModel;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Rooms;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Notifications;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class NotificationCallback : INotificationCallback
{
    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged;

    public event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived;

    public event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted;

    public event EventHandler<PrivateMessageReceivedEventArgs>? PrivateMessageReceived;

    public event EventHandler<RoomUpdatedEventArgs>? RoomUpdated;

    public event EventHandler<RoomClosedEventArgs>? RoomClosed;

    public void ReceiveNotification(Notification notification)
    {
        if (TryDispatchChatNotification(notification)
            || TryDispatchMatchEndNotification(notification)
            || TryDispatchFriendNotification(notification)
            || TryDispatchPrivateMessageNotification(notification)
            || TryDispatchRoomNotification(notification))
        {
            return;
        }

        DispatchMatchmakingNotification(notification);
    }

    private static ChatMessageReceivedEventArgs ToEventArgs(ChatMessageNotification notification)
    {
        return new ChatMessageReceivedEventArgs(ChatContractTranslator.ToChatMessage(notification));
    }

    private static MatchChatMessageReceivedEventArgs ToEventArgs(MatchChatMessageNotification notification)
    {
        return new MatchChatMessageReceivedEventArgs(
            notification.MatchId,
            ChatContractTranslator.ToChatMessage(notification));
    }

    private static MatchAbandonedEventArgs ToEventArgs(MatchAbandonedNotification notification)
    {
        return new MatchAbandonedEventArgs(
            notification.MatchId,
            notification.AbandoningNickname ?? string.Empty,
            notification.IsMatchOver);
    }

    private static MatchFoundEventArgs ToEventArgs(MatchFoundNotification notification)
    {
        MatchFoundParticipant[] notifiedParticipants =
            notification.Participants ?? Array.Empty<MatchFoundParticipant>();
        FoundMatchParticipant[] participants = notifiedParticipants.Select(ToFoundMatchParticipant).ToArray();
        var format = new FoundMatchFormat(
            notification.BoardSize,
            notification.WallsPerPlayer,
            notification.ClockMinutes);
        return new MatchFoundEventArgs(notification.MatchId, format, participants);
    }

    private static FoundMatchParticipant ToFoundMatchParticipant(MatchFoundParticipant participant)
    {
        return new FoundMatchParticipant(
            participant.Nickname ?? string.Empty,
            participant.EloRating,
            participant.TurnOrder);
    }

    private static PresenceRequestedEventArgs ToEventArgs(PresenceRequestedNotification notification)
    {
        return new PresenceRequestedEventArgs(notification.Deadline - notification.OccurredAt);
    }

    private static SearchEndedEventArgs ToEventArgs(SearchEndedNotification notification)
    {
        return new SearchEndedEventArgs(ToReason(notification.Reason));
    }

    private static PrivateMessageReceivedEventArgs ToEventArgs(PrivateMessageNotification notification)
    {
        return new PrivateMessageReceivedEventArgs(
            ConversationContractTranslator.ToSender(notification),
            ConversationContractTranslator.ToPrivateMessage(notification));
    }

    private static FriendPresenceChangedEventArgs ToEventArgs(FriendPresenceNotification notification)
    {
        return new FriendPresenceChangedEventArgs(
            FriendContractTranslator.ToListedFriend(notification),
            notification.OccurredAt);
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

    private bool TryDispatchChatNotification(Notification notification)
    {
        if (notification is ChatMessageNotification chatMessage)
        {
            OnChatMessageReceived(ToEventArgs(chatMessage));
            return true;
        }

        if (notification is MatchChatMessageNotification matchChatMessage)
        {
            OnMatchChatMessageReceived(ToEventArgs(matchChatMessage));
            return true;
        }

        return false;
    }

    private bool TryDispatchMatchEndNotification(Notification notification)
    {
        if (notification is not MatchAbandonedNotification matchAbandoned)
        {
            return false;
        }

        OnMatchAbandoned(ToEventArgs(matchAbandoned));
        return true;
    }

    private bool TryDispatchFriendNotification(Notification notification)
    {
        if (notification is FriendPresenceNotification friendPresence)
        {
            OnFriendPresenceChanged(ToEventArgs(friendPresence));
            return true;
        }

        if (notification is FriendRequestNotification friendRequest)
        {
            PendingFriendRequest request = FriendContractTranslator.ToPendingRequest(friendRequest);
            OnFriendRequestReceived(new FriendRequestReceivedEventArgs(request));
            return true;
        }

        if (notification is FriendshipStartedNotification friendshipStarted)
        {
            var friend = FriendContractTranslator.ToPlayerTag(friendshipStarted);
            OnFriendshipStarted(new FriendshipStartedEventArgs(friend));
            return true;
        }

        return false;
    }

    private bool TryDispatchPrivateMessageNotification(Notification notification)
    {
        if (notification is not PrivateMessageNotification privateMessageNotification)
        {
            return false;
        }

        OnPrivateMessageReceived(ToEventArgs(privateMessageNotification));
        return true;
    }

    private bool TryDispatchRoomNotification(Notification notification)
    {
        if (notification is RoomUpdatedNotification { Room: not null } roomUpdated)
        {
            OnRoomUpdated(new RoomUpdatedEventArgs(RoomContractTranslator.ToPrivateRoom(roomUpdated.Room)));
            return true;
        }

        if (notification is RoomClosedNotification roomClosed)
        {
            RoomCloseReason reason = RoomContractTranslator.ToCloseReason(roomClosed.Reason);
            OnRoomClosed(new RoomClosedEventArgs(roomClosed.Code ?? string.Empty, reason));
            return true;
        }

        return false;
    }

    private void DispatchMatchmakingNotification(Notification notification)
    {
        if (notification is MatchFoundNotification matchFound)
        {
            OnMatchFound(ToEventArgs(matchFound));
            return;
        }

        if (notification is PresenceRequestedNotification presenceRequested)
        {
            OnPresenceRequested(ToEventArgs(presenceRequested));
            return;
        }

        if (notification is SearchEndedNotification searchEnded)
        {
            OnSearchEnded(ToEventArgs(searchEnded));
        }
    }

    private void OnChatMessageReceived(ChatMessageReceivedEventArgs e)
    {
        ChatMessageReceived?.Invoke(this, e);
    }

    private void OnMatchChatMessageReceived(MatchChatMessageReceivedEventArgs e)
    {
        MatchChatMessageReceived?.Invoke(this, e);
    }

    private void OnMatchAbandoned(MatchAbandonedEventArgs e)
    {
        MatchAbandoned?.Invoke(this, e);
    }

    private void OnMatchFound(MatchFoundEventArgs e)
    {
        MatchFound?.Invoke(this, e);
    }

    private void OnPresenceRequested(PresenceRequestedEventArgs e)
    {
        PresenceRequested?.Invoke(this, e);
    }

    private void OnSearchEnded(SearchEndedEventArgs e)
    {
        SearchEnded?.Invoke(this, e);
    }

    private void OnFriendPresenceChanged(FriendPresenceChangedEventArgs e)
    {
        FriendPresenceChanged?.Invoke(this, e);
    }

    private void OnFriendRequestReceived(FriendRequestReceivedEventArgs e)
    {
        FriendRequestReceived?.Invoke(this, e);
    }

    private void OnFriendshipStarted(FriendshipStartedEventArgs e)
    {
        FriendshipStarted?.Invoke(this, e);
    }

    private void OnPrivateMessageReceived(PrivateMessageReceivedEventArgs e)
    {
        PrivateMessageReceived?.Invoke(this, e);
    }

    private void OnRoomUpdated(RoomUpdatedEventArgs e)
    {
        RoomUpdated?.Invoke(this, e);
    }

    private void OnRoomClosed(RoomClosedEventArgs e)
    {
        RoomClosed?.Invoke(this, e);
    }
}
