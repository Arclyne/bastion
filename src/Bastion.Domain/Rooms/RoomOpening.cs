using System;

namespace Bastion.Domain.Rooms;

public sealed record RoomOpening(string Code, int HostId, DateTime OpenedAt);
