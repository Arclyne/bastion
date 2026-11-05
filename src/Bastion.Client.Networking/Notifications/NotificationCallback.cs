using System;
using System.Linq;
using System.ServiceModel;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Notifications;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class NotificationCallback : INotificationCallback
{
    public event EventHandler<LobbyChatMessageReceivedEventArgs>? LobbyChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged;

    public event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived;

    public event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted;

    public void ReceiveNotification(Notification notification)
    {
        // An exception here would fault the duplex channel and stop every notification, so any other type is ignored.
        if (TryDispatchChatNotification(notification) || TryDispatchFriendNotification(notification))
        {
            return;
        }

        DispatchMatchmakingNotification(notification);
    }

    private static LobbyChatMessageReceivedEventArgs ToEventArgs(LobbyChatMessageNotification notification)
    {
        return new LobbyChatMessageReceivedEventArgs(
            notification.SenderNickname ?? string.Empty,
            notification.Text ?? string.Empty,
            notification.OccurredAt);
    }

    private static MatchChatMessageReceivedEventArgs ToEventArgs(MatchChatMessageNotification notification)
    {
        return new MatchChatMessageReceivedEventArgs(
            notification.MatchId,
            notification.SenderNickname ?? string.Empty,
            notification.Text ?? string.Empty);
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
        // Both times come from the server clock, so the countdown does not depend on the client clock.
        return new PresenceRequestedEventArgs(notification.Deadline - notification.OccurredAt);
    }

    private static SearchEndedEventArgs ToEventArgs(SearchEndedNotification notification)
    {
        return new SearchEndedEventArgs(ToReason(notification.Reason));
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
        if (notification is LobbyChatMessageNotification lobbyChatMessage)
        {
            OnLobbyChatMessageReceived(ToEventArgs(lobbyChatMessage));
            return true;
        }

        if (notification is MatchChatMessageNotification matchChatMessage)
        {
            OnMatchChatMessageReceived(ToEventArgs(matchChatMessage));
            return true;
        }

        return false;
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

    private void OnLobbyChatMessageReceived(LobbyChatMessageReceivedEventArgs e)
    {
        LobbyChatMessageReceived?.Invoke(this, e);
    }

    private void OnMatchChatMessageReceived(MatchChatMessageReceivedEventArgs e)
    {
        MatchChatMessageReceived?.Invoke(this, e);
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
}
