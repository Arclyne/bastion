using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Screens.MainMenu.Friends;

public static class FriendRejectionText
{
    public static string Describe(FriendshipRejection rejection)
    {
        switch (rejection)
        {
            case FriendshipRejection.PlayerNotFound:
                return Strings.FriendsRejectedNotFound;
            case FriendshipRejection.OwnAccount:
                return Strings.FriendsItsYou;
            case FriendshipRejection.AlreadyFriends:
                return Strings.FriendsAlreadyFriend;
            case FriendshipRejection.RequestAlreadySent:
                return Strings.FriendsRejectedAlreadySent;
            case FriendshipRejection.FriendLimitReached:
                return Strings.FriendsRejectedOwnLimit;
            case FriendshipRejection.RecipientFriendLimitReached:
                return Strings.FriendsRejectedTheirLimit;
            case FriendshipRejection.RequestNotFound:
                return Strings.FriendsRejectedRequestNotFound;
            case FriendshipRejection.NotFriends:
                return Strings.FriendsRejectedNotFriends;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported friendship rejection.");
        }
    }
}
