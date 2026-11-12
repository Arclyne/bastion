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

        string ownFriendCode = list.OwnFriendCode ?? string.Empty;
        FriendListEntry[] entries = list.Friends ?? Array.Empty<FriendListEntry>();
        IEnumerable<ListedFriend> friends = entries.Select(ToListedFriend);
        var friendList = new List<ListedFriend>(friends);
        return new FriendListing(ownFriendCode, friendList, list.ListedAt);
    }

    public static ListedFriend ToListedFriend(FriendPresenceNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        PlayerTag friend = ToPlayerTag(notification.FriendNickname, notification.FriendCode);
        FriendPresence presence = ToPresence(notification.Presence);
        return new ListedFriend(friend, presence, notification.LastSeenAt);
    }

    public static IReadOnlyList<FoundPlayer> ToFoundPlayers(PlayerSearchEntry[]? entries)
    {
        PlayerSearchEntry[] searchEntries = entries ?? Array.Empty<PlayerSearchEntry>();
        IEnumerable<FoundPlayer> foundPlayers = searchEntries.Select(ToFoundPlayer);
        return new List<FoundPlayer>(foundPlayers);
    }

    public static FriendRequestListing ToFriendRequestListing(FriendRequestList list)
    {
        ArgumentNullException.ThrowIfNull(list);

        List<PendingFriendRequest> receivedRequests = ToPendingRequests(list.ReceivedRequests);
        List<PendingFriendRequest> sentRequests = ToPendingRequests(list.SentRequests);
        return new FriendRequestListing(receivedRequests, sentRequests, list.ListedAt);
    }

    public static PendingFriendRequest ToPendingRequest(FriendRequestNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        PlayerTag requester = ToPlayerTag(notification.RequesterNickname, notification.RequesterFriendCode);
        return new PendingFriendRequest(requester, notification.OccurredAt, notification.ExpiresAt);
    }

    public static PlayerTag ToPlayerTag(FriendshipStartedNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return ToPlayerTag(notification.FriendNickname, notification.FriendCode);
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
            case FriendshipRejectionReason.RequestNotFound:
                return FriendshipRejection.RequestNotFound;
            case FriendshipRejectionReason.NotFriends:
                return FriendshipRejection.NotFriends;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported rejection reason.");
        }
    }

    private static ListedFriend ToListedFriend(FriendListEntry entry)
    {
        PlayerTag friend = ToPlayerTag(entry.Nickname, entry.FriendCode);
        FriendPresence presence = ToPresence(entry.Presence);
        return new ListedFriend(friend, presence, entry.LastSeenAt);
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

    private static FoundPlayer ToFoundPlayer(PlayerSearchEntry entry)
    {
        PlayerTag player = ToPlayerTag(entry.Nickname, entry.FriendCode);
        FriendRelationship relationship = ToRelationship(entry.Relationship);
        return new FoundPlayer(player, relationship);
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

    private static List<PendingFriendRequest> ToPendingRequests(FriendRequestEntry[]? entries)
    {
        FriendRequestEntry[] requestEntries = entries ?? Array.Empty<FriendRequestEntry>();
        IEnumerable<PendingFriendRequest> pendingRequests = requestEntries.Select(ToPendingRequest);
        return new List<PendingFriendRequest>(pendingRequests);
    }

    private static PendingFriendRequest ToPendingRequest(FriendRequestEntry entry)
    {
        PlayerTag player = ToPlayerTag(entry.Nickname, entry.FriendCode);
        return new PendingFriendRequest(player, entry.RequestedAt, entry.ExpiresAt);
    }

    private static PlayerTag ToPlayerTag(string? nickname, string? friendCode)
    {
        string tagNickname = nickname ?? string.Empty;
        string tagFriendCode = friendCode ?? string.Empty;
        return new PlayerTag(tagNickname, tagFriendCode);
    }
}
