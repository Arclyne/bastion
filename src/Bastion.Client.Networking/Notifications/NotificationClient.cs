using System;
using System.ServiceModel;
using System.Threading;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts;
using Bastion.Contracts.Sessions;
using log4net;

namespace Bastion.Client.Networking.Notifications;

public sealed class NotificationClient : INotificationClient, IDisposable
{
    private const int KeepAliveSeconds = 60;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationClient));
    private static readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(KeepAliveSeconds);

    private readonly NotificationCallback _callback = new NotificationCallback();
    private readonly DuplexChannelFactory<ISessionService> _factory;
    private readonly TimeProvider _timeProvider;
    private ISessionService? _channel;
    private ITimer? _keepAliveTimer;

    public NotificationClient(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
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

    public event EventHandler<MatchFoundEventArgs>? MatchFound
    {
        add => _callback.MatchFound += value;
        remove => _callback.MatchFound -= value;
    }

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested
    {
        add => _callback.PresenceRequested += value;
        remove => _callback.PresenceRequested -= value;
    }

    public event EventHandler<SearchEndedEventArgs>? SearchEnded
    {
        add => _callback.SearchEnded += value;
        remove => _callback.SearchEnded -= value;
    }

    public event EventHandler? Disconnected;

    public void Connect(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Disconnect();
        ISessionService channel = _factory.CreateChannel();
        try
        {
            channel.Subscribe(sessionToken);
        }
        catch (CommunicationException ex)
        {
            throw AbortSubscription(channel, ex);
        }
        catch (TimeoutException ex)
        {
            throw AbortSubscription(channel, ex);
        }

        ((ICommunicationObject)channel).Faulted += HandleChannelFaulted;
        _channel = channel;
        _keepAliveTimer = _timeProvider.CreateTimer(SendKeepAlive, null, _keepAliveInterval, _keepAliveInterval);
    }

    public void Disconnect()
    {
        TryStopKeepAlive();
        if (_channel is null)
        {
            return;
        }

        var communicationObject = (ICommunicationObject)_channel;
        communicationObject.Faulted -= HandleChannelFaulted;
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
        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void SendKeepAlive(object? state)
    {
        ISessionService? channel = _channel;
        if (channel is null)
        {
            return;
        }

        try
        {
            channel.KeepAlive();
        }
        catch (CommunicationException ex)
        {
            ReportConnectionLost(ex);
        }
        catch (TimeoutException ex)
        {
            ReportConnectionLost(ex);
        }
        catch (ObjectDisposedException ex)
        {
            ReportConnectionLost(ex);
        }
    }

    private void ReportConnectionLost(Exception cause)
    {
        _logger.Warn("The keep-alive could not reach the server.", cause);
        RaiseDisconnected();
    }

    private void HandleChannelFaulted(object? sender, EventArgs e)
    {
        _logger.Warn("The notification channel faulted.");
        RaiseDisconnected();
    }

    private void RaiseDisconnected()
    {
        // Stopping the keep-alive is the guard: only the first report after a connection raises the event.
        if (!TryStopKeepAlive())
        {
            return;
        }

        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private bool TryStopKeepAlive()
    {
        ITimer? timer = Interlocked.Exchange(ref _keepAliveTimer, null);
        if (timer is null)
        {
            return false;
        }

        timer.Dispose();
        return true;
    }
}
