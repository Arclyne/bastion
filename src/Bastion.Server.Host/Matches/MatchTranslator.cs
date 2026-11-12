using System;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Matches;
using Bastion.Server.Host.Common;
using CoreWCF;

namespace Bastion.Server.Host.Matches;

public static class MatchTranslator
{
    public static FaultException<MatchNotJoinedFault> ToFault(MatchNotJoinedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new MatchNotJoinedFault();
        return FaultExceptionFactory.Create(fault, rejection);
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
