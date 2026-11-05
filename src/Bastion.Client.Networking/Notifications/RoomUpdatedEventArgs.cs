using System;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Networking.Notifications;

public sealed class RoomUpdatedEventArgs : EventArgs
{
    public RoomUpdatedEventArgs(PrivateRoom room)
    {
        ArgumentNullException.ThrowIfNull(room);

        Room = room;
    }

    public PrivateRoom Room { get; }
}
