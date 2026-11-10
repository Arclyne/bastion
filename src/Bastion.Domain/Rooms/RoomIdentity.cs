using System;

namespace Bastion.Domain.Rooms;

public sealed record RoomIdentity
{
    public RoomIdentity(int roomId, string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        RoomId = roomId;
        Code = code;
    }

    public int RoomId { get; }

    public string Code { get; }
}
