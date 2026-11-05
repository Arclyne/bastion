using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public interface IFriendRequestService
{
    IReadOnlyList<PlayerSearchResult> SearchPlayers(string sessionToken, string text, DateTime now);

    FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode, DateTime now);

    FriendRequestBoard ListFriendRequests(string sessionToken, DateTime now);

    void AcceptFriendRequest(string sessionToken, string requesterFriendCode, DateTime now);

    void RejectFriendRequest(string sessionToken, string requesterFriendCode, DateTime now);

    void CancelFriendRequest(string sessionToken, string recipientFriendCode, DateTime now);
}
