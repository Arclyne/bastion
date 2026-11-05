using System;

namespace Bastion.Server.Application.Friends;

public sealed record FriendSummary(PlayerIdentity Friend, DateTime? LastSeenAt);
