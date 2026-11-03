using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking;

public sealed class ServerConnection : IDisposable
{
    private readonly AccountClient _accounts = new AccountClient();
    private readonly NotificationClient _notifications = new NotificationClient();
    private readonly ChatClient _chat = new ChatClient();

    public ServerConnection()
    {
        Accounts = _accounts;
        Notifications = _notifications;
        Chat = _chat;
    }

    public IAccountClient Accounts { get; }

    public INotificationClient Notifications { get; }

    public IChatClient Chat { get; }

    public void Dispose()
    {
        _notifications.Dispose();
        _chat.Dispose();
        _accounts.Dispose();
    }
}
