using System;
using System.ServiceModel;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Matches;

public static class MatchContractTranslator
{
    public static PawnMove ToPawnMove(ChosenPawnMove move)
    {
        ArgumentNullException.ThrowIfNull(move);

        return new PawnMove
        {
            MatchId = move.MatchId,
            MoveNumber = move.MoveNumber,
            ToSquare = move.ToSquare,
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
}
