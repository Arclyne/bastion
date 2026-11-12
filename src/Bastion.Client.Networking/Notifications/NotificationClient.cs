using System;
using System.ServiceModel;
using System.Threading;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts.Sessions;
using log4net;

namespace Bastion.Client.Networking.Notifications;

public sealed class NotificationClient : INotificationClient, IDisposable
{
    private const int KeepAliveSeconds = 60;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationClient));
    private static readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(KeepAliveSeconds);

    private readonly NotificationCallback _notificationCallback;
    private readonly ISessionChannelFactory _sessionChannelFactory;
    private readonly TimeProvider _timeProvider;
    private ISessionService? _sessionChannel;
    private ITimer? _keepAliveTimer;

    public NotificationClient(
        NotificationCallback notificationCallback,
        ISessionChannelFactory sessionChannelFactory,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(notificationCallback);
        ArgumentNullException.ThrowIfNull(sessionChannelFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _notificationCallback = notificationCallback;
        _sessionChannelFactory = sessionChannelFactory;
        _timeProvider = timeProvider;
    }

    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived
    {
        add => _notificationCallback.ChatMessageReceived += value;
        remove => _notificationCallback.ChatMessageReceived -= value;
    }

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived
    {
        add => _notificationCallback.MatchChatMessageReceived += value;
        remove => _notificationCallback.MatchChatMessageReceived -= value;
    }

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned
    {
        add => _notificationCallback.MatchAbandoned += value;
        remove => _notificationCallback.MatchAbandoned -= value;
    }

    public event EventHandler<MatchFoundEventArgs>? MatchFound
    {
        add => _notificationCallback.MatchFound += value;
        remove => _notificationCallback.MatchFound -= value;
    }

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested
    {
        add => _notificationCallback.PresenceRequested += value;
        remove => _notificationCallback.PresenceRequested -= value;
    }

    public event EventHandler<SearchEndedEventArgs>? SearchEnded
    {
        add => _notificationCallback.SearchEnded += value;
        remove => _notificationCallback.SearchEnded -= value;
    }

    public event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged
    {
        add => _notificationCallback.FriendPresenceChanged += value;
        remove => _notificationCallback.FriendPresenceChanged -= value;
    }

    public event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived
    {
        add => _notificationCallback.FriendRequestReceived += value;
        remove => _notificationCallback.FriendRequestReceived -= value;
    }

    public event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted
    {
        add => _notificationCallback.FriendshipStarted += value;
        remove => _notificationCallback.FriendshipStarted -= value;
    }

    public event EventHandler<PrivateMessageReceivedEventArgs>? PrivateMessageReceived
    {
        add => _notificationCallback.PrivateMessageReceived += value;
        remove => _notificationCallback.PrivateMessageReceived -= value;
    }

    public event EventHandler<RoomUpdatedEventArgs>? RoomUpdated
    {
        add => _notificationCallback.RoomUpdated += value;
        remove => _notificationCallback.RoomUpdated -= value;
    }

    public event EventHandler<RoomClosedEventArgs>? RoomClosed
    {
        add => _notificationCallback.RoomClosed += value;
        remove => _notificationCallback.RoomClosed -= value;
    }

    public event EventHandler? Disconnected;

    public void Connect(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Disconnect();
        ISessionService channel = _sessionChannelFactory.CreateChannel();
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

        var communicationObject = (ICommunicationObject)channel;
        communicationObject.Faulted += HandleChannelFaulted;
        _sessionChannel = channel;
        _keepAliveTimer = _timeProvider.CreateTimer(SendKeepAlive, null, _keepAliveInterval, _keepAliveInterval);
    }

    public void Disconnect()
    {
        TryStopKeepAlive();
        if (_sessionChannel is null)
        {
            return;
        }

        var communicationObject = (ICommunicationObject)_sessionChannel;
        communicationObject.Faulted -= HandleChannelFaulted;
        _sessionChannel = null;
        ChannelCloser.CloseOrAbort(communicationObject);
    }

    public void Dispose()
    {
        Disconnect();
    }

    private static Exception AbortSubscription(ISessionService channel, Exception ex)
    {
        var communicationObject = (ICommunicationObject)channel;
        communicationObject.Abort();
        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void SendKeepAlive(object? state)
    {
        ISessionService? channel = _sessionChannel;
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
