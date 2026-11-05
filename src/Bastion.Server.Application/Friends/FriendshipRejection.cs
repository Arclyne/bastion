namespace Bastion.Server.Application.Friends;

public enum FriendshipRejection
{
    PlayerNotFound,
    OwnAccount,
    AlreadyFriends,
    RequestAlreadySent,
    FriendLimitReached,
    RecipientFriendLimitReached,
    RequestNotFound,
    NotFriends,
}
