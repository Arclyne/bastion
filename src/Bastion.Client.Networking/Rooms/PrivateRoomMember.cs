namespace Bastion.Client.Networking.Rooms;

public sealed record PrivateRoomMember(string Nickname, int SeatIndex, bool IsReady, bool IsHost);
