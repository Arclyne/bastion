using System;
using System.Linq;
using Bastion.Contracts.Friends;

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
