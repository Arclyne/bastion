using System.Collections.Generic;

namespace Bastion.Client.Networking.Friends;

public interface IFriendClient
{
    FriendListing ListFriends(string sessionToken);

    IReadOnlyList<FoundPlayer> SearchPlayers(string sessionToken, string text);

    FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode);
}
