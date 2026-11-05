using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public sealed class PlayerSearchEntry
{
    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public string? FriendCode { get; set; }

    [DataMember]
    public PlayerRelationship Relationship { get; set; }
}
