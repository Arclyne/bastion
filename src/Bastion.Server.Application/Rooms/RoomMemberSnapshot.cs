namespace Bastion.Server.Application.Rooms;

public sealed record RoomMemberSnapshot
{
    public required int AccountId { get; init; }

    public required string Nickname { get; init; }

    public required int SeatIndex { get; init; }

    public required bool IsReady { get; init; }

    public required bool IsHost { get; init; }
}
