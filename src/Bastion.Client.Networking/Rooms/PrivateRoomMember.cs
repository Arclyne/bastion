namespace Bastion.Client.Networking.Rooms;

public sealed record PrivateRoomMember
{
    public required string Nickname { get; init; }

    public required int SeatIndex { get; init; }

    public required bool IsReady { get; init; }

    public required bool IsHost { get; init; }
}
