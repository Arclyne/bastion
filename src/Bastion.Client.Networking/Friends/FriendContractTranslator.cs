using System;
using System.Linq;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Friends;

public static class FriendContractTranslator
{
    public static FriendListing ToFriendListing(FriendList list)
    {
        ArgumentNullException.ThrowIfNull(list);

        FriendListEntry[] entries = list.Friends ?? Array.Empty<FriendListEntry>();
        return new FriendListing(
            list.OwnFriendCode ?? string.Empty,
            entries.Select(ToListedFriend).ToList(),
            list.ListedAt);
    }

    public static ListedFriend ToListedFriend(FriendPresenceNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var friend = new PlayerTag(
            notification.FriendNickname ?? string.Empty,
            notification.FriendCode ?? string.Empty);
        return new ListedFriend(friend, ToPresence(notification.Presence), notification.LastSeenAt);
    }

    private static ListedFriend ToListedFriend(FriendListEntry entry)
    {
        var friend = new PlayerTag(entry.Nickname ?? string.Empty, entry.FriendCode ?? string.Empty);
        return new ListedFriend(friend, ToPresence(entry.Presence), entry.LastSeenAt);
    }

    private static FriendPresence ToPresence(PresenceState state)
    {
        switch (state)
        {
            case PresenceState.Connected:
                return FriendPresence.Connected;
            case PresenceState.Disconnected:
                return FriendPresence.Disconnected;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Unsupported presence state.");
        }
    }
}
