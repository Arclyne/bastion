using System;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomRejectedException : Exception
{
    public RoomRejectedException(RoomRejection rejection)
        : this(rejection, TimeSpan.Zero)
    {
    }

    public RoomRejectedException(RoomRejection rejection, TimeSpan retryDelay)
        : base($"The room request was rejected. Rejection={rejection}")
    {
        Rejection = rejection;
        RetryDelay = retryDelay;
    }

    public RoomRejection Rejection { get; }

    public TimeSpan RetryDelay { get; }
}
