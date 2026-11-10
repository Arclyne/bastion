using System;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomRepositoryException : Exception
{
    private const string DefaultMessage = "The room repository could not complete the operation.";

    public RoomRepositoryException()
        : base(DefaultMessage)
    {
    }

    public RoomRepositoryException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public RoomRepositoryException(string? message)
        : base(message)
    {
    }

    public RoomRepositoryException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
