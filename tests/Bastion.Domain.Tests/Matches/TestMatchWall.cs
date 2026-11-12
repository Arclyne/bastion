using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestMatchWall
{
    private const int FirstAccountId = 3;
    private const int SecondAccountId = 6;
    private const short FirstEloRating = 1055;
    private const short SecondEloRating = 1000;
    private const byte ClockMinutes = 5;
    private const byte TwoPlayers = 2;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte SingleWallPerPlayer = 1;
    private const short FirstMoveNumber = 1;
    private const short SecondMoveNumber = 2;
    private const short ThirdMoveNumber = 3;
    private const string CenterGrooveNotation = "e4";
    private const string SecondPlayerForwardSquare = "e8";
    private const string FirstPlayerStartingGroove = "e1";
    private const string FirstPlayerForwardSquare = "e2";
    private const byte ClassicWallsAfterOne = 9;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 12, 18, 0, 0, DateTimeKind.Utc);
    private static readonly MatchEntrant[] _entrants = new MatchEntrant[]
    {
        new MatchEntrant(FirstAccountId, FirstEloRating),
        new MatchEntrant(SecondAccountId, SecondEloRating),
    };

    private static readonly Wall _centerWall =
        new Wall(BoardLayout.ParseSquare(CenterGrooveNotation), WallOrientation.Horizontal);
    private static readonly Wall _wallAheadOfFirstPawn =
        new Wall(BoardLayout.ParseSquare(FirstPlayerStartingGroove), WallOrientation.Horizontal);

    [Fact]
    public void PlaceWall_FreeGroove_SpendsOneWall()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);

        PlaceFirstPlayerWall(match, _centerWall);

        Assert.Equal(ClassicWallsAfterOne, FindParticipation(match, FirstAccountId).RemainingWalls);
    }

    [Fact]
    public void PlaceWall_FreeGroove_PassesTurn()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);

        PlaceFirstPlayerWall(match, _centerWall);

        Assert.Equal(Match.SecondTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void PlaceWall_FreeGroove_RecordsWallMove()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);

        PlaceFirstPlayerWall(match, _centerWall);

        Assert.Equal(_centerWall, match.Moves.Single().GetWall());
    }

    [Fact]
    public void PlaceWall_FreeGroove_RecordsGrooveInBoardNotation()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);

        PlaceFirstPlayerWall(match, _centerWall);

        Assert.Equal(CenterGrooveNotation, match.Moves.Single().Groove);
    }

    [Fact]
    public void BuildBoard_WallPlaced_ContainsWall()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);
        PlaceFirstPlayerWall(match, _centerWall);

        Board board = match.BuildBoard(ClassicBoardSize);

        Assert.True(board.HasWall(_centerWall));
    }

    [Fact]
    public void ListReachableSquares_WallAheadOfPawn_ExcludesSquareBehindWall()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);
        PlaceFirstPlayerWall(match, _wallAheadOfFirstPawn);

        IReadOnlyList<BoardCoordinates> reachableSquares = match.ListReachableSquares(FirstAccountId, ClassicBoardSize);

        Assert.DoesNotContain(BoardLayout.ParseSquare(FirstPlayerForwardSquare), reachableSquares);
    }

    [Fact]
    public void FindWallRejection_LastWallSpent_RejectsNoWallsLeft()
    {
        Match match = StartMatch(SingleWallPerPlayer);
        PlaceFirstPlayerWall(match, _centerWall);
        MoveSecondPlayerForward(match);
        var placement = new WallPlacement(FirstAccountId, ThirdMoveNumber, _wallAheadOfFirstPawn);

        WallRejection? rejection = match.FindWallRejection(placement, ClassicBoardSize);

        Assert.Equal(new WallRejection(WallRejectionReason.NoWallsLeft, null), rejection);
    }

    [Fact]
    public void PlaceWall_GrooveTaken_Throws()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);
        PlaceFirstPlayerWall(match, _centerWall);
        var placement = new WallPlacement(SecondAccountId, SecondMoveNumber, _centerWall);

        Exception? failure = Record.Exception(() => match.PlaceWall(placement, ClassicBoardSize, _startedAt));

        Assert.IsType<InvalidOperationException>(failure);
    }

    [Fact]
    public void PlaceWall_RivalTurn_Throws()
    {
        Match match = StartMatch(ClassicWallsPerPlayer);
        var placement = new WallPlacement(SecondAccountId, FirstMoveNumber, _centerWall);

        Exception? failure = Record.Exception(() => match.PlaceWall(placement, ClassicBoardSize, _startedAt));

        Assert.IsType<InvalidOperationException>(failure);
    }

    private static Match StartMatch(byte wallsPerPlayer)
    {
        var gameMode = new GameMode
        {
            Code = GameModeCode.Classic,
            BoardSize = ClassicBoardSize,
            PlayerCount = TwoPlayers,
            WallsPerPlayer = wallsPerPlayer,
            IsActive = true,
        };
        var format = new MatchFormat(gameMode, ClockMinutes);
        return Match.StartRanked(format, _entrants, _startedAt);
    }

    private static void PlaceFirstPlayerWall(Match match, Wall wall)
    {
        var placement = new WallPlacement(FirstAccountId, FirstMoveNumber, wall);
        match.PlaceWall(placement, ClassicBoardSize, _startedAt);
    }

    private static void MoveSecondPlayerForward(Match match)
    {
        BoardCoordinates destination = BoardLayout.ParseSquare(SecondPlayerForwardSquare);
        var pawnMove = new PawnMove(SecondAccountId, SecondMoveNumber, destination);
        match.MovePawn(pawnMove, ClassicBoardSize, _startedAt);
    }

    private static Participation FindParticipation(Match match, int accountId)
    {
        return match.Participations.Single(participation => participation.AccountId == accountId);
    }
}
