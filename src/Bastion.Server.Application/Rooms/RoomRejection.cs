namespace Bastion.Server.Application.Rooms;

public enum RoomRejection
{
    NotificationsUnavailable,
    UnfinishedMatch,
    SearchingMatch,
    InRoom,
    InvalidSettings,
    RoomNotFound,
    RoomFull,
    Kicked,
    TooManyAttempts,
    TooManyPlayers,
    NotInRoom,
    NotHost,
    NotReadyToStart,
    MatchStarted,
}
