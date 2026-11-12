using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class WallPlacedSampleFactory
{
    public const byte WallsLeftAfterOne = 9;

    public static WallPlacedEventArgs CreateFirstWall(string placerNickname, string groove)
    {
        return new WallPlacedEventArgs
        {
            MatchId = JoinedMatchSampleFactory.MatchId,
            PlacerNickname = placerNickname,
            Groove = groove,
            Orientation = PlacedWallOrientation.Horizontal,
            RemainingWalls = WallsLeftAfterOne,
            NextTurnOrder = Match.SecondTurnOrder,
            NextMoveNumber = PawnMovedSampleFactory.SecondMoveNumber,
            RemainingClockMilliseconds = null,
        };
    }
}
