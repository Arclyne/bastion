using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Friends;
using CoreWCF;

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

    public static PlayerSearchEntry[] ToSearchEntries(IReadOnlyList<PlayerSearchResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.Select(ToSearchEntry).ToArray();
    }

    public static FriendRequestResult ToResult(FriendRequestOutcome outcome)
    {
        switch (outcome)
        {
            case FriendRequestOutcome.RequestSent:
                return FriendRequestResult.RequestSent;
            case FriendRequestOutcome.FriendshipStarted:
                return FriendRequestResult.FriendshipStarted;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unsupported friend request outcome.");
        }
    }

    public static FriendRequestList ToFriendRequestList(FriendRequestBoard board)
    {
        ArgumentNullException.ThrowIfNull(board);

        return new FriendRequestList
        {
            Received = board.Received.Select(ToRequestEntry).ToArray(),
            Sent = board.Sent.Select(ToRequestEntry).ToArray(),
            ListedAt = board.ListedAt,
        };
    }

    public static FriendRequestNotification ToRequestNotification(FriendRequestSummary request, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new FriendRequestNotification
        {
            OccurredAt = now,
            RequesterNickname = request.Counterpart.Nickname,
            RequesterFriendCode = request.Counterpart.FriendCode,
            ExpiresAt = request.ExpiresAt,
        };
    }

    public static FriendshipStartedNotification ToFriendshipStartedNotification(PlayerIdentity friend, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(friend);

        return new FriendshipStartedNotification
        {
            OccurredAt = now,
            FriendNickname = friend.Nickname,
            FriendCode = friend.FriendCode,
        };
    }

    public static FaultException<FriendshipRejectedFault> ToFault(FriendshipRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new FriendshipRejectedFault
        {
            Reason = ToReason(rejection.Rejection),
        };
        return new FaultException<FriendshipRejectedFault>(fault, new FaultReason(rejection.Message));
    }

    private static FriendRequestEntry ToRequestEntry(FriendRequestSummary request)
    {
        return new FriendRequestEntry
        {
            Nickname = request.Counterpart.Nickname,
            FriendCode = request.Counterpart.FriendCode,
            RequestedAt = request.RequestedAt,
            ExpiresAt = request.ExpiresAt,
        };
    }

    private static PlayerSearchEntry ToSearchEntry(PlayerSearchResult result)
    {
        return new PlayerSearchEntry
        {
            Nickname = result.Player.Nickname,
            FriendCode = result.Player.FriendCode,
            Relationship = ToRelationship(result.Relationship),
        };
    }

    private static PlayerRelationship ToRelationship(FriendRelationship relationship)
    {
        switch (relationship)
        {
            case FriendRelationship.None:
                return PlayerRelationship.NotRelated;
            case FriendRelationship.RequestSent:
                return PlayerRelationship.RequestSent;
            case FriendRelationship.RequestReceived:
                return PlayerRelationship.RequestReceived;
            case FriendRelationship.AlreadyFriends:
                return PlayerRelationship.Friends;
            case FriendRelationship.OwnAccount:
                return PlayerRelationship.Self;
            default:
                throw new ArgumentOutOfRangeException(nameof(relationship), relationship, "Unsupported relationship.");
        }
    }

    private static FriendshipRejectionReason ToReason(FriendshipRejection rejection)
    {
        switch (rejection)
        {
            case FriendshipRejection.PlayerNotFound:
                return FriendshipRejectionReason.PlayerNotFound;
            case FriendshipRejection.OwnAccount:
                return FriendshipRejectionReason.OwnAccount;
            case FriendshipRejection.AlreadyFriends:
                return FriendshipRejectionReason.AlreadyFriends;
            case FriendshipRejection.RequestAlreadySent:
                return FriendshipRejectionReason.RequestAlreadySent;
            case FriendshipRejection.FriendLimitReached:
                return FriendshipRejectionReason.FriendLimitReached;
            case FriendshipRejection.RecipientFriendLimitReached:
                return FriendshipRejectionReason.RecipientFriendLimitReached;
            case FriendshipRejection.RequestNotFound:
                return FriendshipRejectionReason.RequestNotFound;
            case FriendshipRejection.NotFriends:
                return FriendshipRejectionReason.NotFriends;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported rejection.");
        }
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
