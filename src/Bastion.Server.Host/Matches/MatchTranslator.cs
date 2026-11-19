using System;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;
using Bastion.Server.Application.Matches;
using Bastion.Server.Host.Common;
using CoreWCF;
using DomainAbandonmentPreview = Bastion.Domain.Matches.AbandonmentPreview;
using DomainDrawOfferRejectionReason = Bastion.Domain.Matches.DrawOfferRejectionReason;
using DomainMatchEndReason = Bastion.Domain.Matches.MatchEndReason;
using DomainParticipationResult = Bastion.Domain.Matches.ParticipationResult;
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

    public static FaultException<DrawOfferRejectedFault> ToFault(DrawOfferRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        DrawOfferRejectionReason reason = ToContractReason(rejection.Reason);
        var fault = new DrawOfferRejectedFault
        {
            Reason = reason,
            MovesLeft = rejection.MovesLeft,
        };
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static FaultException<DrawNotPendingFault> ToFault(DrawNotPendingException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new DrawNotPendingFault();
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static FaultException<IllegalWallFault> CreateUnreadableWallFault()
    {
        var rejection = new IllegalWallException(DomainWallRejectionReason.OutOfBoard, blockedTurnOrder: null);
        return ToFault(rejection);
    }

    public static ChosenPawnMove ToChosenPawnMove(PawnMove pawnMove)
    {
        ArgumentNullException.ThrowIfNull(pawnMove);

        string destinationSquare = pawnMove.ToSquare ?? string.Empty;
        return new ChosenPawnMove(pawnMove.MatchId, pawnMove.MoveNumber, destinationSquare);
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

    public static AbandonmentPreview ToContractPreview(DomainAbandonmentPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        return new AbandonmentPreview
        {
            IsRanked = preview.IsRanked,
            InitialElo = preview.InitialElo,
            FinalElo = preview.FinalElo,
            Placement = preview.Placement,
            ActivePlayerCount = preview.ActivePlayerCount,
        };
    }

    public static MatchEndedNotification ToMatchEndedNotification(
        int matchId,
        MatchEndNotice notice,
        DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(notice);

        MatchEndReason endReason = ToContractEndReason(notice.EndReason);
        MatchStanding[] standings = ContractArray.ConvertAll(notice.Standings, ToMatchStanding);
        return new MatchEndedNotification
        {
            MatchId = matchId,
            EndReason = endReason,
            DecidingTurnOrder = notice.DecidingTurnOrder,
            IsRanked = notice.IsRanked,
            MoveCount = notice.MoveCount,
            PlayedSeconds = notice.PlayedSeconds,
            Standings = standings,
            OccurredAt = occurredAt,
        };
    }

    public static ParticipationEndedNotification ToParticipationEndedNotification(
        int matchId,
        ParticipationEndNotice notice,
        DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(notice);

        MatchEndReason endReason = ToContractEndReason(notice.EndReason);
        MatchStanding[] standings = ContractArray.ConvertAll(notice.Standings, ToMatchStanding);
        return new ParticipationEndedNotification
        {
            MatchId = matchId,
            EndReason = endReason,
            DepartingTurnOrder = notice.DepartingTurnOrder,
            ActivePlayerCount = notice.ActivePlayerCount,
            NextTurnOrder = notice.NextTurnOrder,
            NextMoveNumber = notice.NextMoveNumber,
            MoveCount = notice.MoveCount,
            PlayedSeconds = notice.PlayedSeconds,
            Standings = standings,
            OccurredAt = occurredAt,
        };
    }

    private static DrawOfferRejectionReason ToContractReason(DomainDrawOfferRejectionReason reason)
    {
        switch (reason)
        {
            case DomainDrawOfferRejectionReason.NotTwoPlayers:
                return DrawOfferRejectionReason.NotTwoPlayers;
            case DomainDrawOfferRejectionReason.AlreadyOffered:
                return DrawOfferRejectionReason.AlreadyOffered;
            case DomainDrawOfferRejectionReason.TooSoon:
                return DrawOfferRejectionReason.TooSoon;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported draw offer rejection.");
        }
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

    public static WallOrientation ToContractOrientation(DomainWallOrientation orientation)
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

    private static MatchEndReason ToContractEndReason(DomainMatchEndReason endReason)
    {
        switch (endReason)
        {
            case DomainMatchEndReason.Goal:
                return MatchEndReason.Goal;
            case DomainMatchEndReason.Timeout:
                return MatchEndReason.Timeout;
            case DomainMatchEndReason.Resignation:
                return MatchEndReason.Resignation;
            case DomainMatchEndReason.Draw:
                return MatchEndReason.Draw;
            case DomainMatchEndReason.Abandonment:
                return MatchEndReason.Abandonment;
            default:
                throw new ArgumentOutOfRangeException(nameof(endReason), endReason, "Unsupported match end reason.");
        }
    }

    private static MatchStanding ToMatchStanding(PlayerStanding standing)
    {
        var matchStanding = new MatchStanding
        {
            TurnOrder = standing.TurnOrder,
            Placement = standing.Placement,
            InitialElo = standing.InitialElo,
            FinalElo = standing.FinalElo,
            RemainingWalls = standing.RemainingWalls,
        };
        if (standing.Result is DomainParticipationResult result)
        {
            matchStanding.Result = ToContractResult(result);
        }

        if (standing.EndReason is DomainMatchEndReason endReason)
        {
            matchStanding.EndReason = ToContractEndReason(endReason);
        }

        return matchStanding;
    }

    private static ParticipationResult ToContractResult(DomainParticipationResult result)
    {
        switch (result)
        {
            case DomainParticipationResult.Won:
                return ParticipationResult.Won;
            case DomainParticipationResult.Lost:
                return ParticipationResult.Lost;
            case DomainParticipationResult.Draw:
                return ParticipationResult.Draw;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported participation result.");
        }
    }
}
