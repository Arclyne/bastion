using System;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Matches;
using Bastion.Server.Host.Common;
using CoreWCF;
using DomainWallOrientation = Bastion.Domain.Matches.WallOrientation;
using DomainWallRejectionReason = Bastion.Domain.Matches.WallRejectionReason;

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

    public static FaultException<IllegalWallFault> ToFault(IllegalWallException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        WallRejectionReason reason = ToContractReason(rejection.Reason);
        var fault = new IllegalWallFault
        {
            Reason = reason,
            BlockedTurnOrder = rejection.BlockedTurnOrder,
        };
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static FaultException<IllegalWallFault> ToUnreadableWallFault()
    {
        var rejection = new IllegalWallException(DomainWallRejectionReason.OutOfBoard, blockedTurnOrder: null);
        return ToFault(rejection);
    }

    public static ChosenPawnMove ToChosenPawnMove(PawnMove move)
    {
        ArgumentNullException.ThrowIfNull(move);

        string destinationSquare = move.ToSquare ?? string.Empty;
        return new ChosenPawnMove(move.MatchId, move.MoveNumber, destinationSquare);
    }

    public static ChosenWall ToChosenWall(WallPlacement wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        string groove = wall.Groove ?? string.Empty;
        DomainWallOrientation orientation = ToDomainOrientation(wall.Orientation);
        return new ChosenWall
        {
            MatchId = wall.MatchId,
            MoveNumber = wall.MoveNumber,
            Groove = groove,
            Orientation = orientation,
        };
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

    public static WallPlacedNotification ToWallPlacedNotification(
        int matchId,
        WallPlacementNotice notice,
        DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(notice);

        WallOrientation orientation = ToContractOrientation(notice.Orientation);
        return new WallPlacedNotification
        {
            MatchId = matchId,
            PlacerNickname = notice.PlacerNickname,
            Groove = notice.Groove,
            Orientation = orientation,
            RemainingWalls = notice.RemainingWalls,
            NextTurnOrder = notice.NextTurnOrder,
            NextMoveNumber = notice.NextMoveNumber,
            RemainingClockMilliseconds = notice.RemainingClockMilliseconds,
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

    private static WallRejectionReason ToContractReason(DomainWallRejectionReason reason)
    {
        switch (reason)
        {
            case DomainWallRejectionReason.NoWallsLeft:
                return WallRejectionReason.NoWallsLeft;
            case DomainWallRejectionReason.OutOfBoard:
                return WallRejectionReason.OutOfBoard;
            case DomainWallRejectionReason.GrooveTaken:
                return WallRejectionReason.GrooveTaken;
            case DomainWallRejectionReason.WouldBlockPlayer:
                return WallRejectionReason.WouldBlockPlayer;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported wall rejection reason.");
        }
    }

    private static DomainWallOrientation ToDomainOrientation(WallOrientation orientation)
    {
        switch (orientation)
        {
            case WallOrientation.Horizontal:
                return DomainWallOrientation.Horizontal;
            case WallOrientation.Vertical:
                return DomainWallOrientation.Vertical;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(orientation),
                    orientation,
                    "Unsupported wall orientation.");
        }
    }

    private static WallOrientation ToContractOrientation(DomainWallOrientation orientation)
    {
        switch (orientation)
        {
            case DomainWallOrientation.Horizontal:
                return WallOrientation.Horizontal;
            case DomainWallOrientation.Vertical:
                return WallOrientation.Vertical;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(orientation),
                    orientation,
                    "Unsupported wall orientation.");
        }
    }
}
