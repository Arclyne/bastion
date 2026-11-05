using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public interface IFriendRequestService
{
    IReadOnlyList<PlayerSearchResult> SearchPlayers(string sessionToken, string text, DateTime now);

    FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode, DateTime now);
}
