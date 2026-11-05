using System;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Matches;
using CoreWCF;

namespace Bastion.Server.Host.Matches;

public static class MatchTranslator
{
    public static FaultException<MatchNotJoinedFault> ToFault(MatchNotJoinedException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new FaultException<MatchNotJoinedFault>(new MatchNotJoinedFault(), new FaultReason(exception.Message));
    }

    public static MatchAbandonedNotification ToAbandonedNotification(
        int matchId,
        AbandonmentNotice notice,
        DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(notice);

        return new MatchAbandonedNotification
        {
            MatchId = matchId,
            AbandoningNickname = notice.AbandoningNickname,
            IsMatchOver = notice.IsMatchOver,
            OccurredAt = occurredAt,
        };
    }
}
