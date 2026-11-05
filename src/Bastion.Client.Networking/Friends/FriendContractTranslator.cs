using System;
using System.Collections.Generic;
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

    public static IReadOnlyList<FoundPlayer> ToFoundPlayers(PlayerSearchEntry[]? entries)
    {
        return (entries ?? Array.Empty<PlayerSearchEntry>()).Select(ToFoundPlayer).ToList();
    }

    public static FriendRequestOutcome ToOutcome(FriendRequestResult result)
    {
        switch (result)
        {
            case FriendRequestResult.RequestSent:
                return FriendRequestOutcome.RequestSent;
            case FriendRequestResult.FriendshipStarted:
                return FriendRequestOutcome.FriendshipStarted;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported friend request result.");
        }
    }

    public static FriendshipRejection ToRejection(FriendshipRejectionReason reason)
    {
        switch (reason)
        {
            case FriendshipRejectionReason.PlayerNotFound:
                return FriendshipRejection.PlayerNotFound;
            case FriendshipRejectionReason.OwnAccount:
                return FriendshipRejection.OwnAccount;
            case FriendshipRejectionReason.AlreadyFriends:
                return FriendshipRejection.AlreadyFriends;
            case FriendshipRejectionReason.RequestAlreadySent:
                return FriendshipRejection.RequestAlreadySent;
            case FriendshipRejectionReason.FriendLimitReached:
                return FriendshipRejection.FriendLimitReached;
            case FriendshipRejectionReason.RecipientFriendLimitReached:
                return FriendshipRejection.RecipientFriendLimitReached;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported rejection reason.");
        }
    }

    private static FoundPlayer ToFoundPlayer(PlayerSearchEntry entry)
    {
        var player = new PlayerTag(entry.Nickname ?? string.Empty, entry.FriendCode ?? string.Empty);
        return new FoundPlayer(player, ToRelationship(entry.Relationship));
    }

    private static FriendRelationship ToRelationship(PlayerRelationship relationship)
    {
        switch (relationship)
        {
            case PlayerRelationship.NotRelated:
                return FriendRelationship.None;
            case PlayerRelationship.RequestSent:
                return FriendRelationship.RequestSent;
            case PlayerRelationship.RequestReceived:
                return FriendRelationship.RequestReceived;
            case PlayerRelationship.Friends:
                return FriendRelationship.AlreadyFriends;
            case PlayerRelationship.Self:
                return FriendRelationship.OwnAccount;
            default:
                throw new ArgumentOutOfRangeException(nameof(relationship), relationship, "Unsupported relationship.");
        }
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
