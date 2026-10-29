using System;
using System.ServiceModel;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Notifications;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class NotificationReceiver : INotificationCallback
{
    public event EventHandler<ChatMessage>? ChatMessageReceived;

    public void ReceiveNotification(Notification notification)
    {
        if (notification is ChatMessageNotification chatMessage)
        {
            ChatMessageReceived?.Invoke(this, ToChatMessage(chatMessage));
        }
    }

    private static ChatMessage ToChatMessage(ChatMessageNotification notification)
    {
        return new ChatMessage(
            notification.SenderNickname ?? string.Empty,
            notification.Text ?? string.Empty,
            notification.OccurredAt);
    }
}
