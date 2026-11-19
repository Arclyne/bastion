using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Matches;

public static class MatchContractTranslator
{
    public static PawnMove ToPawnMove(ChosenPawnMove chosenMove)
    {
        ArgumentNullException.ThrowIfNull(chosenMove);

        return new PawnMove
        {
            MatchId = chosenMove.MatchId,
            MoveNumber = chosenMove.MoveNumber,
            ToSquare = chosenMove.ToSquare,
        };
    }

    public static WallPlacement ToWallPlacement(ChosenWall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        WallOrientation orientation = ToContractOrientation(wall.Orientation);
        return new WallPlacement
        {
            MatchId = wall.MatchId,
            MoveNumber = wall.MoveNumber,
            Groove = wall.Groove,
            Orientation = orientation,
        };
    }

    public static IllegalWallException ToIllegalWallException(FaultException<IllegalWallFault> fault)
    {
        ArgumentNullException.ThrowIfNull(fault);

        WallPlacementRejection rejection = ToRejection(fault.Detail.Reason);
        return new IllegalWallException(rejection, fault.Detail.BlockedTurnOrder, fault);
    }

    public static PlacedWallOrientation ToPlacedOrientation(WallOrientation orientation)
    {
        switch (orientation)
        {
            case WallOrientation.Horizontal:
                return PlacedWallOrientation.Horizontal;
            case WallOrientation.Vertical:
                return PlacedWallOrientation.Vertical;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(orientation),
                    orientation,
                    "Unsupported wall orientation.");
        }
    }

    private static WallOrientation ToContractOrientation(PlacedWallOrientation orientation)
    {
        switch (orientation)
        {
            case PlacedWallOrientation.Horizontal:
                return WallOrientation.Horizontal;
            case PlacedWallOrientation.Vertical:
                return WallOrientation.Vertical;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(orientation),
                    orientation,
                    "Unsupported wall orientation.");
        }
    }

    private static WallPlacementRejection ToRejection(WallRejectionReason reason)
    {
        switch (reason)
        {
            case WallRejectionReason.NoWallsLeft:
                return WallPlacementRejection.NoWallsLeft;
            case WallRejectionReason.OutOfBoard:
                return WallPlacementRejection.OutOfBoard;
            case WallRejectionReason.GrooveTaken:
                return WallPlacementRejection.GrooveTaken;
            case WallRejectionReason.WouldBlockPlayer:
                return WallPlacementRejection.WouldBlockPlayer;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported wall rejection reason.");
        }
    }

    public static PreviewedAbandonment ToPreviewedAbandonment(AbandonmentPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        return new PreviewedAbandonment
        {
            IsRanked = preview.IsRanked,
            InitialElo = preview.InitialElo,
            FinalElo = preview.FinalElo,
            Placement = preview.Placement,
            ActivePlayerCount = preview.ActivePlayerCount,
        };
    }

    public static EndedMatchReason ToEndedMatchReason(MatchEndReason endReason)
    {
        switch (endReason)
        {
            case MatchEndReason.Goal:
                return EndedMatchReason.Goal;
            case MatchEndReason.Timeout:
                return EndedMatchReason.Timeout;
            case MatchEndReason.Resignation:
                return EndedMatchReason.Resignation;
            case MatchEndReason.Draw:
                return EndedMatchReason.Draw;
            case MatchEndReason.Abandonment:
                return EndedMatchReason.Abandonment;
            default:
                throw new ArgumentOutOfRangeException(nameof(endReason), endReason, "Unsupported match end reason.");
        }
    }

    public static IReadOnlyList<PlayerStanding> ToPlayerStandings(MatchStanding[]? standings)
    {
        MatchStanding[] notifiedStandings = standings ?? Array.Empty<MatchStanding>();
        IEnumerable<PlayerStanding> playerStandings = notifiedStandings.Select(ToPlayerStanding);
        return [.. playerStandings];
    }

    private static PlayerStanding ToPlayerStanding(MatchStanding standing)
    {
        StandingResult result = ToStandingResult(standing.Result);
        var playerStanding = new PlayerStanding
        {
            TurnOrder = standing.TurnOrder,
            Result = result,
            Placement = standing.Placement,
            EndReason = null,
            InitialElo = standing.InitialElo,
            FinalElo = standing.FinalElo,
            RemainingWalls = standing.RemainingWalls,
        };
        if (standing.EndReason is not MatchEndReason notifiedReason)
        {
            return playerStanding;
        }

        EndedMatchReason endReason = ToEndedMatchReason(notifiedReason);
        return playerStanding with { EndReason = endReason };
    }

    private static StandingResult ToStandingResult(ParticipationResult? result)
    {
        switch (result)
        {
            case null:
                return StandingResult.StillPlaying;
            case ParticipationResult.Won:
                return StandingResult.Won;
            case ParticipationResult.Lost:
                return StandingResult.Lost;
            case ParticipationResult.Draw:
                return StandingResult.Draw;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported participation result.");
        }
    }
}
