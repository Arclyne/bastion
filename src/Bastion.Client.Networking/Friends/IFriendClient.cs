using System.Collections.Generic;

namespace Bastion.Client.Networking.Friends;

public interface IFriendClient
{
    FriendListing ListFriends(string sessionToken);

    IReadOnlyList<FoundPlayer> SearchPlayers(string sessionToken, string text);

    FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode);

    FriendRequestListing ListFriendRequests(string sessionToken);

    void AcceptFriendRequest(string sessionToken, string requesterFriendCode);

    void RejectFriendRequest(string sessionToken, string requesterFriendCode);

    void CancelFriendRequest(string sessionToken, string recipientFriendCode);
}
