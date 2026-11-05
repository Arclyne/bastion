using System;

namespace Bastion.Client.Networking.Friends;

public sealed record PendingFriendRequest(PlayerTag Player, DateTime RequestedAt, DateTime ExpiresAt);
