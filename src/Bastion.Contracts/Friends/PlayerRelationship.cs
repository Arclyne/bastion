using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public enum PlayerRelationship
{
    [EnumMember]
    NotRelated,

    [EnumMember]
    RequestSent,

    [EnumMember]
    RequestReceived,

    [EnumMember]
    Friends,

    [EnumMember]
    Self,
}
