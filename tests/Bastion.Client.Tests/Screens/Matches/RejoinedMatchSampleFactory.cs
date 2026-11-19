using System;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class RejoinedMatchSampleFactory
{
    public const string RivalForwardSquare = "e8";
    public const string WallGroove = "e4";
    public const short NextMoveNumberAfterTwoMoves = 3;

    private const string OwnStartingSquare = "e1";
    private const byte WallsAfterOne = JoinedMatchSampleFactory.WallsPerPlayer - 1;

    private static readonly TimeSpan _fourMinutes = TimeSpan.FromMinutes(4);
    private static readonly TimeSpan _fiveMinutes = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan _halfMinute = TimeSpan.FromSeconds(30);

    public static RejoinedMatch CreateAfterRivalWallAndMove(JoinedMatch joinedMatch)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);

        return CreateWithRivalConnection(joinedMatch, isRivalConnected: true);
    }

    public static RejoinedMatch CreateWithRivalConnection(JoinedMatch joinedMatch, bool isRivalConnected)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);

        byte ownTurnOrder = joinedMatch.GetOwnTurnOrder();
        byte rivalTurnOrder = ownTurnOrder == Match.FirstTurnOrder ? Match.SecondTurnOrder : Match.FirstTurnOrder;
        TimeSpan rivalTimeAway = isRivalConnected ? TimeSpan.Zero : _halfMinute;
        var own = new RejoinedPlayer
        {
            TurnOrder = ownTurnOrder,
            Square = OwnStartingSquare,
            RemainingWalls = JoinedMatchSampleFactory.WallsPerPlayer,
            RemainingClock = _fiveMinutes,
            HasEnded = false,
            IsConnected = true,
            TimeAway = TimeSpan.Zero,
        };
        var rival = new RejoinedPlayer
        {
            TurnOrder = rivalTurnOrder,
            Square = RivalForwardSquare,
            RemainingWalls = WallsAfterOne,
            RemainingClock = _fourMinutes,
            HasEnded = false,
            IsConnected = isRivalConnected,
            TimeAway = rivalTimeAway,
        };
        RejoinedMove[] moves =
        [
            CreateMove(Match.FirstMoveNumber, rivalTurnOrder, isWall: true),
            CreateMove(PawnMovedSampleFactory.SecondMoveNumber, rivalTurnOrder, isWall: false),
        ];
        return new RejoinedMatch
        {
            FoundMatch = joinedMatch.FoundMatch,
            Players = [own, rival],
            Moves = moves,
            CurrentTurnOrder = ownTurnOrder,
            NextMoveNumber = NextMoveNumberAfterTwoMoves,
            TurnElapsed = TimeSpan.Zero,
            PendingDrawOffererTurnOrder = 0,
            DrawMovesLeft = 0,
        };
    }

    public static RejoinedMatch CreateEndedByRivalClaim(JoinedMatch joinedMatch)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);

        RejoinedMatch rejoinedMatch = CreateAfterRivalWallAndMove(joinedMatch);
        byte ownTurnOrder = joinedMatch.GetOwnTurnOrder();
        MatchEndedEventArgs ending = MatchEndSampleFactory.CreateOwnAbandonment(ownTurnOrder);
        return rejoinedMatch with { Ending = ending };
    }

    private static RejoinedMove CreateMove(short moveNumber, byte turnOrder, bool isWall)
    {
        return new RejoinedMove
        {
            MoveNumber = moveNumber,
            TurnOrder = turnOrder,
            IsWall = isWall,
            Groove = isWall ? WallGroove : string.Empty,
            Orientation = PlacedWallOrientation.Horizontal,
        };
    }
}
