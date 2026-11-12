using System;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Host.Sessions;

public interface INotificationChannelRegistry
{
    event EventHandler<PlayerDisconnectedEventArgs>? PlayerDisconnected;

    void RegisterChannel(ConnectedPlayer player, INotificationCallback callback);
}
