namespace Bastion.Server.Application.Rooms;

public sealed record RoomKick
{
    public required RoomSnapshot Room { get; init; }

    public required bool IsMemberKicked { get; init; }

    public required int KickedAccountId { get; init; }
}
