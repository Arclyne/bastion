using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking;

public sealed class ServerClients : IDisposable
{
    private readonly AccountClient _accounts = new AccountClient();
    private readonly NotificationClient _notifications = new NotificationClient();
    private readonly ChatClient _chat = new ChatClient();

    public IAccountClient Accounts => _accounts;

    public INotificationClient Notifications => _notifications;

    public IChatClient Chat => _chat;

    public void Dispose()
    {
        _notifications.Dispose();
        _chat.Dispose();
        _accounts.Dispose();
    }
}
