using System;

namespace Bastion.Server.Application.Friends;

public interface IFriendListService
{
    FriendRoster ListFriends(string sessionToken, DateTime now);
}
