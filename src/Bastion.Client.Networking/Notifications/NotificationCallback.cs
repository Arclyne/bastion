using System;
using System.ServiceModel;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Notifications;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class NotificationCallback : INotificationCallback
{
    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public void ReceiveNotification(Notification notification)
    {
        if (notification is not ChatMessageNotification chatMessage)
        {
            return;
        }

        ChatMessageReceived?.Invoke(this, ToEventArgs(chatMessage));
    }

    private static ChatMessageReceivedEventArgs ToEventArgs(ChatMessageNotification notification)
    {
        return new ChatMessageReceivedEventArgs(
            notification.SenderNickname ?? string.Empty,
            notification.Text ?? string.Empty,
            notification.OccurredAt);
    }
}
