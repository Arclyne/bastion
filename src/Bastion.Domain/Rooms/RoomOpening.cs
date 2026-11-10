using System;

namespace Bastion.Domain.Rooms;

public sealed record RoomOpening
{
    public RoomOpening(string code, int hostAccountId, DateTime openedAt)
    {
        ArgumentNullException.ThrowIfNull(code);

        Code = code;
        HostAccountId = hostAccountId;
        OpenedAt = openedAt;
    }

    public string Code { get; }

    public int HostAccountId { get; }

    public DateTime OpenedAt { get; }
}
