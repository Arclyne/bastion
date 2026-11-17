using System;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomRejectedException : Exception
{
    public RoomRejectedException(RoomRejection rejection)
        : this(rejection, TimeSpan.Zero)
    {
    }

    public RoomRejectedException(RoomRejection rejection, TimeSpan retryDelay)
        : this(rejection, retryDelay, innerException: null)
    {
    }

    public RoomRejectedException(RoomRejection rejection, Exception innerException)
        : this(rejection, TimeSpan.Zero, innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
    }

    private RoomRejectedException(RoomRejection rejection, TimeSpan retryDelay, Exception? innerException)
        : base($"The room request was rejected. Rejection={rejection}", innerException)
    {
        Rejection = rejection;
        RetryDelay = retryDelay;
    }

    public RoomRejection Rejection { get; }

    public TimeSpan RetryDelay { get; }
}
