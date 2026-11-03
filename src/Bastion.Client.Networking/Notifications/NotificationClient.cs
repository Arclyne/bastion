using System;
using System.ServiceModel;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Notifications;

public sealed class NotificationClient : INotificationClient, IDisposable
{
    private readonly NotificationCallback _callback = new NotificationCallback();
    private readonly DuplexChannelFactory<ISessionService> _factory;
    private ISessionService? _channel;

    public NotificationClient()
    {
        _factory = new DuplexChannelFactory<ISessionService>(
            new InstanceContext(_callback),
            ChannelSettings.CreateBinding(),
            ChannelSettings.CreateAddress(ServiceEndpoints.SessionsPath));
        ChannelSettings.ApplyCertificateValidation(_factory);
    }

    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived
    {
        add => _callback.ChatMessageReceived += value;
        remove => _callback.ChatMessageReceived -= value;
    }

    public void Connect(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Disconnect();
        ISessionService channel = _factory.CreateChannel();
        try
        {
            channel.Subscribe(sessionToken);
            _channel = channel;
        }
        catch (CommunicationException ex)
        {
            throw AbortSubscription(channel, ex);
        }
        catch (TimeoutException ex)
        {
            throw AbortSubscription(channel, ex);
        }
    }

    public void Disconnect()
    {
        if (_channel is null)
        {
            return;
        }

        var communicationObject = (ICommunicationObject)_channel;
        _channel = null;
        ChannelCloser.CloseOrAbort(communicationObject);
    }

    public void Dispose()
    {
        Disconnect();
        ChannelCloser.CloseOrAbort(_factory);
    }

    private static Exception AbortSubscription(ISessionService channel, Exception ex)
    {
        ((ICommunicationObject)channel).Abort();
        return ToClientException(ex);
    }

    private static Exception ToClientException(Exception ex)
    {
        switch (ex)
        {
            case EndpointNotFoundException:
                return new ServerUnreachableException(ex);
            case FaultException<InvalidSessionFault>:
                return new InvalidSessionException(ex);
            case FaultException:
                return new ServerErrorException(ex);
            case CommunicationException:
            case TimeoutException:
                return new ConnectionLostException(ex);
            default:
                throw new ArgumentOutOfRangeException(nameof(ex), ex.GetType(), "Unsupported exception type.");
        }
    }
}
