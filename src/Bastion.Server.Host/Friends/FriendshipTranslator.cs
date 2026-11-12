using System;
using System.Collections.Generic;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Bastion.Server.Host.Common;
using CoreWCF;

namespace Bastion.Server.Host.Friends;

public static class FriendshipTranslator
{
    public static FriendList ToFriendList(FriendRoster roster)
    {
        ArgumentNullException.ThrowIfNull(roster);

        FriendListEntry[] friends = ContractArray.ConvertAll(roster.Friends, ToFriendListEntry);
        return new FriendList
        {
            OwnFriendCode = roster.OwnFriendCode,
            Friends = friends,
            ListedAt = roster.ListedAt,
        };
    }

    public static FriendPresenceNotification ToPresenceNotification(FriendPresenceChange change, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(change);

        PresenceState presence = ToPresenceState(change.Presence);
        return new FriendPresenceNotification
        {
            OccurredAt = now,
            FriendNickname = change.Friend.Nickname,
            FriendCode = change.Friend.FriendCode,
            Presence = presence,
            LastSeenAt = change.LastSeenAt,
        };
    }

    public static PlayerSearchEntry[] ToSearchEntries(IReadOnlyList<PlayerSearchResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        PlayerSearchEntry[] entries = ContractArray.ConvertAll(results, ToSearchEntry);
        return entries;
    }

    public static FriendRequestResult ToFriendRequestResult(FriendRequestOutcome outcome)
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

        FriendRequestEntry[] receivedRequests = ContractArray.ConvertAll(board.ReceivedRequests, ToRequestEntry);
        FriendRequestEntry[] sentRequests = ContractArray.ConvertAll(board.SentRequests, ToRequestEntry);
        return new FriendRequestList
        {
            ReceivedRequests = receivedRequests,
            SentRequests = sentRequests,
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
        return FaultExceptionFactory.Create(fault, rejection);
    }

    private static FriendListEntry ToFriendListEntry(FriendStatus status)
    {
        PresenceState presence = ToPresenceState(status.Presence);
        return new FriendListEntry
        {
            Nickname = status.Summary.Friend.Nickname,
            FriendCode = status.Summary.Friend.FriendCode,
            Presence = presence,
            LastSeenAt = status.Summary.LastSeenAt,
        };
    }

    private static PresenceState ToPresenceState(FriendPresence presence)
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

    private static PlayerSearchEntry ToSearchEntry(PlayerSearchResult result)
    {
        PlayerRelationship relationship = ToRelationship(result.Relationship);
        return new PlayerSearchEntry
        {
            Nickname = result.Player.Nickname,
            FriendCode = result.Player.FriendCode,
            Relationship = relationship,
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
}
