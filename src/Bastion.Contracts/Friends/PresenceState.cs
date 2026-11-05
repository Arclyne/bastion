using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public enum PresenceState
{
    [EnumMember]
    Connected,

    [EnumMember]
    Disconnected,
}
