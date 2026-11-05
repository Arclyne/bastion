using System.Runtime.Serialization;

namespace Bastion.Contracts.Rooms;

[DataContract]
public enum RoomGameMode
{
    [EnumMember]
    Classic,

    [EnumMember]
    FourPlayers,

    [EnumMember]
    Quick,
}
