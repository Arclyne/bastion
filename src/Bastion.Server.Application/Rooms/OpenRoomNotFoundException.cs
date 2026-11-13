using System;

namespace Bastion.Server.Application.Rooms;

public sealed class OpenRoomNotFoundException : Exception
{
    private const string DefaultMessage = "No open room has that code.";

    public OpenRoomNotFoundException()
        : base(DefaultMessage)
    {
    }

    public OpenRoomNotFoundException(string? message)
        : base(message)
    {
    }

    public OpenRoomNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
