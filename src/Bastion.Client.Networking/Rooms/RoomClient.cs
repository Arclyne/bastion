using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Rooms;

namespace Bastion.Client.Networking.Rooms;

public sealed class RoomClient : IRoomClient
{
    private readonly IServiceChannel<IRoomService> _channel;
    private readonly INotificationClient _notificationClient;

    public RoomClient(IServiceChannel<IRoomService> channel, INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(notificationClient);

        _channel = channel;
        _notificationClient = notificationClient;
    }

    public event EventHandler<RoomUpdatedEventArgs>? RoomUpdated
    {
        add => _notificationClient.RoomUpdated += value;
        remove => _notificationClient.RoomUpdated -= value;
    }

    public event EventHandler<RoomClosedEventArgs>? RoomClosed
    {
        add => _notificationClient.RoomClosed += value;
        remove => _notificationClient.RoomClosed -= value;
    }

    public event EventHandler? Disconnected
    {
        add => _notificationClient.Disconnected += value;
        remove => _notificationClient.Disconnected -= value;
    }

    public PrivateRoom CreateRoom(string sessionToken, PrivateRoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(options);

        var contractOptions = RoomContractTranslator.ToRoomOptions(options);
        return CallForRoom(service => service.CreateRoom(sessionToken, contractOptions));
    }

    public PrivateRoom JoinRoom(string sessionToken, string code)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(code);

        return CallForRoom(service => service.JoinRoom(sessionToken, code));
    }

    public PrivateRoom SetReady(string sessionToken, bool isReady)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        return CallForRoom(service => service.SetReady(sessionToken, isReady));
    }

    public PrivateRoom ChangeOptions(string sessionToken, PrivateRoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(options);

        var contractOptions = RoomContractTranslator.ToRoomOptions(options);
        return CallForRoom(service => service.ChangeOptions(sessionToken, contractOptions));
    }

    public void StartMatch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Call(service => service.StartMatch(sessionToken));
    }

    public void LeaveRoom(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Call(service => service.LeaveRoom(sessionToken));
    }

    public PrivateRoom? FindOwnRoom(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        RoomState? state = Call(service => service.FindOwnRoom(sessionToken));
        if (state is null)
        {
            return null;
        }

        return RoomContractTranslator.ToPrivateRoom(state);
    }

    private PrivateRoom CallForRoom(Func<IRoomService, RoomState> operation)
    {
        RoomState state = Call(operation);
        return RoomContractTranslator.ToPrivateRoom(state);
    }

    private TResult Call<TResult>(Func<IRoomService, TResult> operation)
    {
        try
        {
            return _channel.Call(operation);
        }
        catch (CommunicationException ex)
        {
            throw ToClientException(ex);
        }
        catch (TimeoutException ex)
        {
            throw ToClientException(ex);
        }
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<RoomRejectedFault> rejected)
        {
            RoomRejection rejection = RoomContractTranslator.ToRejection(rejected.Detail.Reason);
            var retryDelay = TimeSpan.FromSeconds(rejected.Detail.RetryAfterSeconds);
            return new RoomRejectedException(rejection, retryDelay, ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void Call(Action<IRoomService> operation)
    {
        Func<IRoomService, bool> operationAsFunction = service => CompleteOperation(operation, service);
        Call(operationAsFunction);
    }

    private static bool CompleteOperation(Action<IRoomService> operation, IRoomService service)
    {
        operation(service);
        return true;
    }
}
