using System.Collections.Generic;

namespace Bastion.Client.Networking.Rooms;

public sealed record PrivateRoom(
    string Code,
    PrivateRoomOptions Options,
    int SeatCount,
    IReadOnlyList<PrivateRoomMember> Members);
