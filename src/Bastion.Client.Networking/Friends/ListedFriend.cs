using System;

namespace Bastion.Client.Networking.Friends;

public sealed record ListedFriend(PlayerTag Friend, FriendPresence Presence, DateTime? LastSeenAt);
