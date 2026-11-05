namespace Bastion.Server.Application.Rooms;

public sealed record RoomMemberSnapshot(int AccountId, string Nickname, int SeatIndex, bool IsReady, bool IsHost);
