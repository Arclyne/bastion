using System;
using System.Linq;
using Bastion.Contracts.Matchmaking;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Matchmaking;
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
        return new FaultException<SearchRejectedFault>(fault, new FaultReason(rejection.Message));
    }

    public static MatchFoundNotification ToNotification(MatchPairing pairing, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(pairing);

        return new MatchFoundNotification
        {
            OccurredAt = now,
            ParticipantNicknames = pairing.Players.Select(player => player.Nickname).ToArray(),
        };
    }

    public static PresenceRequestedNotification ToNotification(PresenceRequest request, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new PresenceRequestedNotification
        {
            OccurredAt = now,
            Deadline = request.Deadline,
        };
    }

    public static SearchEndedNotification ToNotification(SearchEndReason reason, DateTime now)
    {
        return new SearchEndedNotification
        {
            OccurredAt = now,
            Reason = ToReason(reason),
        };
    }

    private static SearchRejectionReason ToReason(SearchRejection rejection)
    {
        switch (rejection)
        {
            case SearchRejection.NotificationsUnavailable:
                return SearchRejectionReason.NotificationsUnavailable;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported search rejection.");
        }
    }

    private static SearchEndedReason ToReason(SearchEndReason reason)
    {
        switch (reason)
        {
            case SearchEndReason.Cancelled:
                return SearchEndedReason.Cancelled;
            case SearchEndReason.PresenceExpired:
                return SearchEndedReason.PresenceExpired;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported search end reason.");
        }
    }
}
