using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedRoom(RoomSnapshot Room, int OriginAccountId);
