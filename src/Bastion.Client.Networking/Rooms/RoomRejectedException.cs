using System;

namespace Bastion.Client.Networking.Rooms;

public sealed class RoomRejectedException : Exception
{
    public RoomRejectedException(RoomRejection rejection, TimeSpan retryDelay, Exception innerException)
        : base($"The room request was rejected. Rejection={rejection}", innerException)
    {
        Rejection = rejection;
        RetryDelay = retryDelay;
    }

    public RoomRejection Rejection { get; }

    public TimeSpan RetryDelay { get; }
}
