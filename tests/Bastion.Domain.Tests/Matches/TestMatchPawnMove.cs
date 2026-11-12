using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestMatchPawnMove
{
    private const int FirstAccountId = 3;
    private const int SecondAccountId = 6;
    private const short FirstEloRating = 1055;
    private const short SecondEloRating = 1000;
    private const byte ClockMinutes = 5;
    private const int FiveMinutesInMilliseconds = 300000;
    private const int ThirtySecondsInMilliseconds = 30000;
    private const int TenSecondsInMilliseconds = 10000;
    private const byte TwoPlayers = 2;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte QuickBoardSize = 7;
    private const byte QuickWallsPerPlayer = 6;
    private const short FirstMoveNumber = 1;
    private const short SecondMoveNumber = 2;
    private const string FirstPlayerForwardSquare = "e2";
    private const string FirstPlayerTwoAheadSquare = "e3";
    private const string SecondPlayerForwardSquare = "e8";

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 12, 18, 0, 0, DateTimeKind.Utc);
    private static readonly MatchEntrant[] _entrants = new MatchEntrant[]
    {
        new MatchEntrant(FirstAccountId, FirstEloRating),
        new MatchEntrant(SecondAccountId, SecondEloRating),
    };

    private static readonly string[] _quickMatchToFirstPlayerGoal = new string[]
    {
        "d2", "c7", "d3", "c6", "d4", "c5", "d5", "c4", "d6", "c3", "d7",
    };

    [Fact]
    public void GetNextMoveNumber_NoMoves_ReturnsOne()
    {
        Match match = StartClassicMatch();

        short nextMoveNumber = match.GetNextMoveNumber();

        Assert.Equal(FirstMoveNumber, nextMoveNumber);
    }

    [Fact]
    public void IsTurnOf_FirstPlayerAtStart_ReturnsTrue()
    {
        Match match = StartClassicMatch();

        bool isTurn = match.IsTurnOf(FirstAccountId);

        Assert.True(isTurn);
    }

    [Fact]
    public void IsTurnOf_SecondPlayerAtStart_ReturnsFalse()
    {
        Match match = StartClassicMatch();

        bool isTurn = match.IsTurnOf(SecondAccountId);

        Assert.False(isTurn);
    }

    [Fact]
    public void MovePawn_FirstPlayerForward_MovesPawnToDestination()
    {
        Match match = StartClassicMatch();

        MoveFirstPlayerForward(match);

        Assert.Equal(FirstPlayerForwardSquare, FindParticipation(match, FirstAccountId).CurrentSquare);
    }

    [Fact]
    public void MovePawn_FirstPlayerForward_PassesTurnToSecondPlayer()
    {
        Match match = StartClassicMatch();

        MoveFirstPlayerForward(match);

        Assert.Equal(Match.SecondTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void MovePawn_SecondPlayerMoves_GivesTurnBackToFirstPlayer()
    {
        Match match = StartClassicMatch();
        MoveFirstPlayerForward(match);
        var pawnMove = new PawnMove(SecondAccountId, SecondMoveNumber, ParseSquare(SecondPlayerForwardSquare));

        match.MovePawn(pawnMove, ClassicBoardSize, _startedAt.AddMilliseconds(TenSecondsInMilliseconds));

        Assert.Equal(Match.FirstTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void MovePawn_FirstPlayerForward_RecordsMoveFromStartingSquare()
    {
        Match match = StartClassicMatch();

        string startingSquare = BoardLayout.GetStartingSquare(ClassicBoardSize, Match.FirstTurnOrder);

        MoveFirstPlayerForward(match);

        Assert.Equal(startingSquare, match.Moves.Single().FromSquare);
    }

    [Fact]
    public void MovePawn_ThirtySecondsAfterStart_SpendsThoseSecondsFromClock()
    {
        Match match = StartClassicMatch();
        var pawnMove = new PawnMove(FirstAccountId, FirstMoveNumber, ParseSquare(FirstPlayerForwardSquare));

        match.MovePawn(pawnMove, ClassicBoardSize, _startedAt.AddMilliseconds(ThirtySecondsInMilliseconds));

        int expectedClock = FiveMinutesInMilliseconds - ThirtySecondsInMilliseconds;
        Assert.Equal(expectedClock, FindParticipation(match, FirstAccountId).RemainingClockMilliseconds);
    }

    [Fact]
    public void MovePawn_SecondMove_CountsTimeFromPreviousMove()
    {
        Match match = StartClassicMatch();
        MoveFirstPlayerForward(match);
        var pawnMove = new PawnMove(SecondAccountId, SecondMoveNumber, ParseSquare(SecondPlayerForwardSquare));
        DateTime playedAt = _startedAt.AddMilliseconds(TenSecondsInMilliseconds);

        match.MovePawn(pawnMove, ClassicBoardSize, playedAt);

        Assert.Equal(TenSecondsInMilliseconds, match.Moves.Last().TimeSpentMilliseconds);
    }

    [Fact]
    public void MovePawn_SecondPlayerAtStart_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        var pawnMove = new PawnMove(SecondAccountId, FirstMoveNumber, ParseSquare(SecondPlayerForwardSquare));

        Assert.Throws<InvalidOperationException>(() => match.MovePawn(pawnMove, ClassicBoardSize, _startedAt));
    }

    [Fact]
    public void MovePawn_UnexpectedMoveNumber_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        var pawnMove = new PawnMove(FirstAccountId, SecondMoveNumber, ParseSquare(FirstPlayerForwardSquare));

        Assert.Throws<InvalidOperationException>(() => match.MovePawn(pawnMove, ClassicBoardSize, _startedAt));
    }

    [Fact]
    public void MovePawn_SquareTwoAhead_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        var pawnMove = new PawnMove(FirstAccountId, FirstMoveNumber, ParseSquare(FirstPlayerTwoAheadSquare));

        Assert.Throws<InvalidOperationException>(() => match.MovePawn(pawnMove, ClassicBoardSize, _startedAt));
    }

    [Fact]
    public void BuildBoard_AfterFirstMove_PlacesPawnOnDestination()
    {
        Match match = StartClassicMatch();
        MoveFirstPlayerForward(match);

        Board board = match.BuildBoard(ClassicBoardSize);

        Assert.Equal(ParseSquare(FirstPlayerForwardSquare), board.GetPawn(Match.FirstTurnOrder));
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_FinishesMatch()
    {
        Match match = PlayQuickMatchToFirstPlayerGoal();

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_EndsByGoal()
    {
        Match match = PlayQuickMatchToFirstPlayerGoal();

        Assert.Equal(MatchEndReason.Goal, match.EndReason);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_FirstPlayerWins()
    {
        Match match = PlayQuickMatchToFirstPlayerGoal();

        Assert.Equal(ParticipationResult.Won, FindParticipation(match, FirstAccountId).Result);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_SecondPlayerLoses()
    {
        Match match = PlayQuickMatchToFirstPlayerGoal();

        Assert.Equal(ParticipationResult.Lost, FindParticipation(match, SecondAccountId).Result);
    }

    private static Match StartClassicMatch()
    {
        MatchFormat format = CreateFormat(GameModeCode.Classic, ClassicBoardSize, ClassicWallsPerPlayer);
        return Match.StartRanked(format, _entrants, _startedAt);
    }

    private static Match PlayQuickMatchToFirstPlayerGoal()
    {
        MatchFormat format = CreateFormat(GameModeCode.Quick, QuickBoardSize, QuickWallsPerPlayer);
        var match = Match.StartRanked(format, _entrants, _startedAt);
        var accountIdsInTurnOrder = new List<int> { FirstAccountId, SecondAccountId };
        for (int moveIndex = 0; moveIndex < _quickMatchToFirstPlayerGoal.Length; moveIndex++)
        {
            int accountId = accountIdsInTurnOrder[moveIndex % TwoPlayers];
            short moveNumber = match.GetNextMoveNumber();
            BoardCoordinates destination = ParseSquare(_quickMatchToFirstPlayerGoal[moveIndex]);
            var pawnMove = new PawnMove(accountId, moveNumber, destination);
            match.MovePawn(pawnMove, QuickBoardSize, _startedAt.AddSeconds(moveIndex));
        }

        return match;
    }

    private static void MoveFirstPlayerForward(Match match)
    {
        var pawnMove = new PawnMove(FirstAccountId, FirstMoveNumber, ParseSquare(FirstPlayerForwardSquare));
        match.MovePawn(pawnMove, ClassicBoardSize, _startedAt);
    }

    private static BoardCoordinates ParseSquare(string square)
    {
        return BoardLayout.ParseSquare(square);
    }

    private static Participation FindParticipation(Match match, int accountId)
    {
        return match.Participations.Single(participation => participation.AccountId == accountId);
    }

    private static MatchFormat CreateFormat(GameModeCode code, byte boardSize, byte wallsPerPlayer)
    {
        var gameMode = new GameMode
        {
            Code = code,
            BoardSize = boardSize,
            PlayerCount = TwoPlayers,
            WallsPerPlayer = wallsPerPlayer,
            IsActive = true,
        };
        return new MatchFormat(gameMode, ClockMinutes);
    }
}
