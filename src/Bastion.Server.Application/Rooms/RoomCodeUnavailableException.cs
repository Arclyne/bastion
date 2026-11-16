using System;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomCodeUnavailableException : Exception
{
    private const string DefaultMessage = "No free room code was found.";

    public RoomCodeUnavailableException()
        : base(DefaultMessage)
    {
    }

    public RoomCodeUnavailableException(string? message)
        : base(message)
    {
    }

    public RoomCodeUnavailableException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
