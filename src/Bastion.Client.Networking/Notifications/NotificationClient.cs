using System;
using System.ServiceModel;
using log4net;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Notifications;

public sealed class NotificationClient : INotificationClient, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationClient));

    private readonly NotificationReceiver _receiver = new NotificationReceiver();
    private readonly DuplexChannelFactory<ISessionService> _factory;
    private ISessionService? _channel;

    public NotificationClient()
    {
        _factory = new DuplexChannelFactory<ISessionService>(
            new InstanceContext(_receiver),
            ChannelSettings.CreateBinding(),
            ChannelSettings.CreateAddress(ServiceEndpoints.SessionsPath));
        ChannelSettings.ApplyCertificateValidation(_factory);
    }

    public event EventHandler<ChatMessage>? ChatMessageReceived
    {
        add => _receiver.ChatMessageReceived += value;
        remove => _receiver.ChatMessageReceived -= value;
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
        catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
        {
            ((ICommunicationObject)channel).Abort();
            throw ToClientException(ex);
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
        if (communicationObject.State == CommunicationState.Opened)
        {
            communicationObject.Close();
            return;
        }

        communicationObject.Abort();
    }

    public void Dispose()
    {
        Disconnect();
        if (_factory.State == CommunicationState.Faulted)
        {
            _factory.Abort();
            return;
        }

        _factory.Close();
    }

    private static Exception ToClientException(Exception ex)
    {
        switch (ex)
        {
            case EndpointNotFoundException:
                _logger.Warn("The server could not be reached.", ex);
                return new ServerUnreachableException(ex);
            case FaultException<InvalidSessionFault>:
                _logger.Warn("The server turned down the session.", ex);
                return new InvalidSessionException(ex);
            case FaultException:
                _logger.Warn("The server could not open the notification channel.", ex);
                return new ServerErrorException(ex);
            default:
                _logger.Warn("The connection with the server was lost.", ex);
                return new ConnectionLostException(ex);
        }
    }
}
