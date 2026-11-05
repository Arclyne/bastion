using System;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Networking.Notifications;

public sealed class RoomClosedEventArgs : EventArgs
{
    public RoomClosedEventArgs(string code, RoomCloseReason reason)
    {
        ArgumentNullException.ThrowIfNull(code);

        Code = code;
        Reason = reason;
    }

    public string Code { get; }

    public RoomCloseReason Reason { get; }
}
