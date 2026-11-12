namespace Bastion.Client.Networking.Rooms;

public enum RoomRejection
{
    NotificationsUnavailable,
    UnfinishedMatch,
    SearchingMatch,
    InRoom,
    InvalidOptions,
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
