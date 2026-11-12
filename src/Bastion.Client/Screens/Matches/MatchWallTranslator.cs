using System;
using Bastion.Client.Networking.Matches;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public static class MatchWallTranslator
{
    public static WallOrientation ToWallOrientation(PlacedWallOrientation orientation)
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

    public static PlacedWallOrientation ToPlacedWallOrientation(WallOrientation orientation)
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

    public static WallRejectionReason ToRejectionReason(WallPlacementRejection rejection)
    {
        switch (rejection)
        {
            case WallPlacementRejection.NoWallsLeft:
                return WallRejectionReason.NoWallsLeft;
            case WallPlacementRejection.OutOfBoard:
                return WallRejectionReason.OutOfBoard;
            case WallPlacementRejection.GrooveTaken:
                return WallRejectionReason.GrooveTaken;
            case WallPlacementRejection.WouldBlockPlayer:
                return WallRejectionReason.WouldBlockPlayer;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported wall rejection.");
        }
    }
}
