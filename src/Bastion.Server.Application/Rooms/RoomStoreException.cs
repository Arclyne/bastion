using System;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomStoreException : Exception
{
    private const string DefaultMessage = "The room store could not complete the operation.";

    public RoomStoreException()
        : base(DefaultMessage)
    {
    }

    public RoomStoreException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public RoomStoreException(string? message)
        : base(message)
    {
    }

    public RoomStoreException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
