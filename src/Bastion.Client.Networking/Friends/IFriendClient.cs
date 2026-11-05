namespace Bastion.Client.Networking.Friends;

public interface IFriendClient
{
    FriendListing ListFriends(string sessionToken);
}
