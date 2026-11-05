using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Rooms;

public interface IRoomClient
{
    event EventHandler<RoomUpdatedEventArgs>? RoomUpdated;

    event EventHandler<RoomClosedEventArgs>? RoomClosed;

    event EventHandler? Disconnected;

    PrivateRoom CreateRoom(string sessionToken, PrivateRoomOptions options);

    PrivateRoom JoinRoom(string sessionToken, string code);

    PrivateRoom SetReady(string sessionToken, bool isReady);

    PrivateRoom ChangeOptions(string sessionToken, PrivateRoomOptions options);

    void LeaveRoom(string sessionToken);

    PrivateRoom? FindOwnRoom(string sessionToken);
}
