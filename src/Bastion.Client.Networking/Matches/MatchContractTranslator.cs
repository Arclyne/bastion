using System;
using System.ServiceModel;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Notifications;

namespace Bastion.Client.Networking.Matches;

public static class MatchContractTranslator
{
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

    public static WallPlacedEventArgs ToWallPlacedEventArgs(WallPlacedNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string placerNickname = notification.PlacerNickname ?? string.Empty;
        string groove = notification.Groove ?? string.Empty;
        PlacedWallOrientation orientation = ToPlacedOrientation(notification.Orientation);
        return new WallPlacedEventArgs
        {
            MatchId = notification.MatchId,
            PlacerNickname = placerNickname,
            Groove = groove,
            Orientation = orientation,
            RemainingWalls = notification.RemainingWalls,
            NextTurnOrder = notification.NextTurnOrder,
            NextMoveNumber = notification.NextMoveNumber,
            RemainingClockMilliseconds = notification.RemainingClockMilliseconds,
        };
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

    private static PlacedWallOrientation ToPlacedOrientation(WallOrientation orientation)
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
