using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts;
using Bastion.Contracts.Rooms;

namespace Bastion.Client.Networking.Rooms;

public sealed class RoomClient : IRoomClient, IDisposable
{
    private readonly ServiceChannel<IRoomService> _channel =
        new ServiceChannel<IRoomService>(ServiceEndpoints.RoomsPath);
    private readonly INotificationClient _notificationClient;

    public RoomClient(INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(notificationClient);

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

        RoomOptions contractOptions = RoomContractTranslator.ToContract(options);
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

        RoomOptions contractOptions = RoomContractTranslator.ToContract(options);
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
        return state is null ? null : RoomContractTranslator.ToPrivateRoom(state);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<RoomRejectedFault> rejected)
        {
            return new RoomRejectedException(
                RoomContractTranslator.ToRejection(rejected.Detail.Reason),
                TimeSpan.FromSeconds(rejected.Detail.RetryAfterSeconds),
                ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private PrivateRoom CallForRoom(Func<IRoomService, RoomState> operation)
    {
        return RoomContractTranslator.ToPrivateRoom(Call(operation));
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

    private void Call(Action<IRoomService> operation)
    {
        try
        {
            _channel.Call(operation);
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
}
