using System.Collections.Generic;

namespace Bastion.Client.Networking.Rooms;

public sealed record PrivateRoom
{
    public required string Code { get; init; }

    public required PrivateRoomOptions Options { get; init; }

    public required int SeatCount { get; init; }

    public required IReadOnlyList<PrivateRoomMember> Members { get; init; }
}
