using System;
using System.ServiceModel;
using Bastion.Contracts;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Channels;

public sealed class SessionChannelFactory : ISessionChannelFactory, IDisposable
{
    private readonly DuplexChannelFactory<ISessionService> _channelFactory;

    public SessionChannelFactory(INotificationCallback notificationCallback)
    {
        ArgumentNullException.ThrowIfNull(notificationCallback);

        var callbackContext = new InstanceContext(notificationCallback);
        NetTcpBinding binding = ChannelConfiguration.CreateBinding();
        EndpointAddress address = ChannelConfiguration.CreateAddress(EndpointCatalog.SessionsPath);
        _channelFactory = new DuplexChannelFactory<ISessionService>(callbackContext, binding, address);
        ChannelConfiguration.ApplyCertificateValidation(_channelFactory);
    }

    public ISessionService CreateChannel()
    {
        return _channelFactory.CreateChannel();
    }

    public void Dispose()
    {
        ChannelCloser.CloseOrAbort(_channelFactory);
    }
}
