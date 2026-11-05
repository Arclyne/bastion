using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public enum RoomRejectionReason
{
    [EnumMember]
    NotificationsUnavailable,

    [EnumMember]
    UnfinishedMatch,

    [EnumMember]
    SearchingMatch,

    [EnumMember]
    InRoom,

    [EnumMember]
    InvalidSettings,

    [EnumMember]
    RoomNotFound,

    [EnumMember]
    RoomFull,

    [EnumMember]
    Kicked,

    [EnumMember]
    TooManyAttempts,

    [EnumMember]
    TooManyPlayers,

    [EnumMember]
    NotInRoom,

    [EnumMember]
    NotHost,
}
