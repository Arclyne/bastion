using System;

namespace Bastion.Client.Networking.Notifications;

public interface INotificationClient
{
    event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    void Connect(string sessionToken);

    void Disconnect();
}
