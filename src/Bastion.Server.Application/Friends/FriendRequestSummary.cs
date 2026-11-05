using System;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRequestSummary(PlayerIdentity Counterpart, DateTime RequestedAt, DateTime ExpiresAt);
