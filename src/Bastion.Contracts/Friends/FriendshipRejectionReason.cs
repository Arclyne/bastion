using System.Runtime.Serialization;

namespace Bastion.Contracts.Friends;

[DataContract]
public enum FriendshipRejectionReason
{
    [EnumMember]
    PlayerNotFound,

    [EnumMember]
    OwnAccount,

    [EnumMember]
    AlreadyFriends,

    [EnumMember]
    RequestAlreadySent,

    [EnumMember]
    FriendLimitReached,

    [EnumMember]
    RecipientFriendLimitReached,

    [EnumMember]
    RequestNotFound,

    [EnumMember]
    NotFriends,
}
