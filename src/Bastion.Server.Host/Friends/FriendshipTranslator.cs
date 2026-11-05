using System;
using System.Linq;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Host.Friends;

public static class FriendshipTranslator
{
    public static FriendList ToFriendList(FriendRoster roster)
    {
        ArgumentNullException.ThrowIfNull(roster);

        return new FriendList
        {
            OwnFriendCode = roster.OwnFriendCode,
            Friends = roster.Friends.Select(ToEntry).ToArray(),
            ListedAt = roster.ListedAt,
        };
    }

    public static FriendPresenceNotification ToNotification(FriendPresenceChange change, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(change);

        return new FriendPresenceNotification
        {
            OccurredAt = now,
            FriendNickname = change.Friend.Nickname,
            FriendCode = change.Friend.FriendCode,
            Presence = ToState(change.Presence),
            LastSeenAt = change.LastSeenAt,
        };
    }

    private static FriendListEntry ToEntry(FriendStatus status)
    {
        return new FriendListEntry
        {
            Nickname = status.Summary.Friend.Nickname,
            FriendCode = status.Summary.Friend.FriendCode,
            Presence = ToState(status.Presence),
            LastSeenAt = status.Summary.LastSeenAt,
        };
    }

    private static PresenceState ToState(FriendPresence presence)
    {
        switch (presence)
        {
            case FriendPresence.Connected:
                return PresenceState.Connected;
            case FriendPresence.Disconnected:
                return PresenceState.Disconnected;
            default:
                throw new ArgumentOutOfRangeException(nameof(presence), presence, "Unsupported friend presence.");
        }
    }
}
