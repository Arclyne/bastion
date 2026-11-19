using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class PawnMovedSampleFactory
{
    public const short SecondMoveNumber = 2;

    public static PawnMovedEventArgs CreateFirstMove(int matchId, string moverNickname, string toSquare)
    {
        return new PawnMovedEventArgs
        {
            MatchId = matchId,
            MoverNickname = moverNickname,
            ToSquare = toSquare,
            NextTurnOrder = Match.SecondTurnOrder,
            NextMoveNumber = SecondMoveNumber,
            RemainingClockMilliseconds = null,
        };
    }
}
