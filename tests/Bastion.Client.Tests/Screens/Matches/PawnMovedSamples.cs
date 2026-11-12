using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class PawnMovedSamples
{
    public const short SecondMoveNumber = 2;

    private const string StartingSquare = "e1";

    public static PawnMovedEventArgs CreateFirstMove(int matchId, string moverNickname, string toSquare)
    {
        return new PawnMovedEventArgs
        {
            MatchId = matchId,
            MoverNickname = moverNickname,
            FromSquare = StartingSquare,
            ToSquare = toSquare,
            NextTurnOrder = Match.SecondTurnOrder,
            NextMoveNumber = SecondMoveNumber,
            RemainingClockMilliseconds = null,
            IsMatchOver = false,
        };
    }

    public static PawnMovedEventArgs CreateWinningMove(string moverNickname, string toSquare)
    {
        return new PawnMovedEventArgs
        {
            MatchId = JoinedMatchSampleFactory.MatchId,
            MoverNickname = moverNickname,
            FromSquare = StartingSquare,
            ToSquare = toSquare,
            NextTurnOrder = Match.SecondTurnOrder,
            NextMoveNumber = SecondMoveNumber,
            RemainingClockMilliseconds = null,
            IsMatchOver = true,
        };
    }
}
