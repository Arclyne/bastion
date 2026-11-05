using System;
using System.ServiceModel;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Notifications;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class NotificationCallback : INotificationCallback
{
    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public void ReceiveNotification(Notification notification)
    {
        // An exception here would fault the duplex channel and stop every notification, so any other type is ignored.
        if (notification is ChatMessageNotification chatMessage)
        {
            ChatMessageReceived?.Invoke(this, ToEventArgs(chatMessage));
            return;
        }

        if (notification is MatchFoundNotification matchFound)
        {
            MatchFound?.Invoke(this, ToEventArgs(matchFound));
            return;
        }

        if (notification is PresenceRequestedNotification presenceRequested)
        {
            PresenceRequested?.Invoke(this, ToEventArgs(presenceRequested));
            return;
        }

        if (notification is SearchEndedNotification searchEnded)
        {
            SearchEnded?.Invoke(this, ToEventArgs(searchEnded));
        }
    }

    private static ChatMessageReceivedEventArgs ToEventArgs(ChatMessageNotification notification)
    {
        return new ChatMessageReceivedEventArgs(
            notification.SenderNickname ?? string.Empty,
            notification.Text ?? string.Empty,
            notification.OccurredAt);
    }

    private static MatchFoundEventArgs ToEventArgs(MatchFoundNotification notification)
    {
        return new MatchFoundEventArgs(notification.ParticipantNicknames ?? Array.Empty<string>());
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

    private static SearchEndReason ToReason(SearchEndedReason reason)
    {
        switch (reason)
        {
            case SearchEndedReason.Cancelled:
                return SearchEndReason.Cancelled;
            case SearchEndedReason.PresenceExpired:
                return SearchEndReason.PresenceExpired;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported search end reason.");
        }
    }
}
