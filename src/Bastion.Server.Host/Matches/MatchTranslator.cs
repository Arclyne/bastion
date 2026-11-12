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

    public static FaultException<NotYourTurnFault> ToFault(NotYourTurnException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new NotYourTurnFault
        {
            CurrentTurnOrder = rejection.CurrentTurnOrder,
            NextMoveNumber = rejection.NextMoveNumber,
        };
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static FaultException<IllegalMoveFault> ToFault(IllegalMoveException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new IllegalMoveFault();
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static ChosenPawnMove ToChosenPawnMove(PawnMove move)
    {
        ArgumentNullException.ThrowIfNull(move);

        string destinationSquare = move.ToSquare ?? string.Empty;
        return new ChosenPawnMove(move.MatchId, move.MoveNumber, destinationSquare);
    }

    public static PawnMovedNotification ToPawnMovedNotification(int matchId, PawnMoveNotice notice, DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(notice);

        return new PawnMovedNotification
        {
            MatchId = matchId,
            MoverNickname = notice.MoverNickname,
            FromSquare = notice.FromSquare,
            ToSquare = notice.ToSquare,
            NextTurnOrder = notice.NextTurnOrder,
            NextMoveNumber = notice.NextMoveNumber,
            RemainingClockMilliseconds = notice.RemainingClockMilliseconds,
            IsMatchOver = notice.IsMatchOver,
            OccurredAt = occurredAt,
        };
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
