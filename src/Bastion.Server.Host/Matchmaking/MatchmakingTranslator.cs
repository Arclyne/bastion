using System;
using Bastion.Contracts.Matchmaking;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Host.Common;
using CoreWCF;

namespace Bastion.Server.Host.Matchmaking;

public static class MatchmakingTranslator
{
    public static FaultException<SearchRejectedFault> ToFault(SearchRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new SearchRejectedFault
        {
            Reason = ToReason(rejection.Rejection),
        };
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static MatchFoundNotification ToMatchFoundNotification(CreatedMatch createdMatch, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(createdMatch);

        MatchFoundParticipant[] participants =
            ContractArray.ConvertAll(createdMatch.Participants, ToMatchFoundParticipant);
        return new MatchFoundNotification
        {
            OccurredAt = now,
            MatchId = createdMatch.MatchId,
            BoardSize = createdMatch.Format.GameMode.BoardSize,
            WallsPerPlayer = createdMatch.Format.WallsPerPlayer,
            ClockMinutes = createdMatch.Format.ClockMinutes,
            Participants = participants,
        };
    }

    public static PresenceRequestedNotification ToPresenceRequestedNotification(PresenceRequest request, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new PresenceRequestedNotification
        {
            OccurredAt = now,
            Deadline = request.Deadline,
        };
    }

    public static SearchEndedNotification ToSearchEndedNotification(SearchEndReason reason, DateTime now)
    {
        SearchEndedReason searchEndedReason = ToReason(reason);
        return new SearchEndedNotification
        {
            OccurredAt = now,
            Reason = searchEndedReason,
        };
    }

    private static SearchRejectionReason ToReason(SearchRejection rejection)
    {
        switch (rejection)
        {
            case SearchRejection.NotificationsUnavailable:
                return SearchRejectionReason.NotificationsUnavailable;
            case SearchRejection.UnfinishedMatch:
                return SearchRejectionReason.UnfinishedMatch;
            case SearchRejection.InRoom:
                return SearchRejectionReason.InRoom;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported search rejection.");
        }
    }

    private static MatchFoundParticipant ToMatchFoundParticipant(CreatedMatchParticipant participant)
    {
        return new MatchFoundParticipant
        {
            Nickname = participant.Player.Nickname,
            EloRating = participant.EloRating,
            TurnOrder = participant.TurnOrder,
        };
    }

    private static SearchEndedReason ToReason(SearchEndReason reason)
    {
        switch (reason)
        {
            case SearchEndReason.Cancelled:
                return SearchEndedReason.Cancelled;
            case SearchEndReason.PresenceExpired:
                return SearchEndedReason.PresenceExpired;
            case SearchEndReason.MatchCreationFailed:
                return SearchEndedReason.MatchCreationFailed;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported search end reason.");
        }
    }
}
