using System;

namespace Bastion.Client.Networking.Notifications;

public interface INotificationClient
{
    event EventHandler<ChatMessage>? ChatMessageReceived;

    void Connect(string sessionToken);

    void Disconnect();
}
