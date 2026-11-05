using System;

namespace Bastion.Server.Application.Friends;

public sealed record FriendPresenceChange(PlayerIdentity Friend, FriendPresence Presence, DateTime? LastSeenAt);
