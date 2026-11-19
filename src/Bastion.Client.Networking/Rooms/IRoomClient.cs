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

    void StartMatch(string sessionToken);

    PrivateRoom KickPlayer(string sessionToken, string nickname);

    void LeaveRoom(string sessionToken);

    OwnRoom FindOwnRoom(string sessionToken);
}
