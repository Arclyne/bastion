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
        Func<IRoomService, RoomState> createRoom = service => service.CreateRoom(sessionToken, contractOptions);
        return CallForRoom(createRoom);
    }

    public PrivateRoom JoinRoom(string sessionToken, string code)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(code);

        Func<IRoomService, RoomState> joinRoom = service => service.JoinRoom(sessionToken, code);
        return CallForRoom(joinRoom);
    }

    public PrivateRoom SetReady(string sessionToken, bool isReady)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Func<IRoomService, RoomState> setReady = service => service.SetReady(sessionToken, isReady);
        return CallForRoom(setReady);
    }

    public PrivateRoom ChangeOptions(string sessionToken, PrivateRoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(options);

        var contractOptions = RoomContractTranslator.ToRoomOptions(options);
        Func<IRoomService, RoomState> changeOptions = service => service.ChangeOptions(sessionToken, contractOptions);
        return CallForRoom(changeOptions);
    }

    public void StartMatch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Action<IRoomService> startMatch = service => service.StartMatch(sessionToken);
        Call(startMatch);
    }

    public PrivateRoom KickPlayer(string sessionToken, string nickname)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(nickname);

        Func<IRoomService, RoomState> kickPlayer = service => service.KickPlayer(sessionToken, nickname);
        return CallForRoom(kickPlayer);
    }

    public void LeaveRoom(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Action<IRoomService> leaveRoom = service => service.LeaveRoom(sessionToken);
        Call(leaveRoom);
    }

    public OwnRoom FindOwnRoom(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Func<IRoomService, OwnRoomResult> findOwnRoom = service => service.FindOwnRoom(sessionToken);
        OwnRoomResult result = Call(findOwnRoom);
        return RoomContractTranslator.ToOwnRoom(result);
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

    private static Exception ToClientException(Exception cause)
    {
        if (cause is FaultException<RoomRejectedFault> rejected)
        {
            RoomRejection rejection = RoomContractTranslator.ToRejection(rejected.Detail.Reason);
            var retryDelay = TimeSpan.FromSeconds(rejected.Detail.RetryAfterSeconds);
            return new RoomRejectedException(rejection, retryDelay, cause);
        }

        return ServiceFailureTranslator.ToClientException(cause);
    }

    private void Call(Action<IRoomService> operation)
    {
        Func<IRoomService, bool> completeOperation = service => CompleteOperation(operation, service);
        Call(completeOperation);
    }

    private static bool CompleteOperation(Action<IRoomService> operation, IRoomService service)
    {
        operation(service);
        return true;
    }
}
