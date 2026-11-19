using System;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed record PrivateRoomServices
{
    public PrivateRoomServices(IRoomClient rooms, IClipboard clipboard, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(clipboard);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Rooms = rooms;
        Clipboard = clipboard;
        TimeProvider = timeProvider;
    }

    public IRoomClient Rooms { get; }

    public IClipboard Clipboard { get; }

    public TimeProvider TimeProvider { get; }
}
