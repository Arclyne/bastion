using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRoster(string OwnFriendCode, IReadOnlyList<FriendStatus> Friends, DateTime ListedAt);
