using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Friends;

public sealed record FriendListing(string OwnFriendCode, IReadOnlyList<ListedFriend> Friends, DateTime ListedAt);
