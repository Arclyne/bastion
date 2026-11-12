using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Tests.GameModes;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public sealed class TestMatch
{
    private const int FirstAccountId = 3;
    private const int SecondAccountId = 6;
    private const short FirstEloRating = 1055;
    private const short SecondEloRating = 1000;
    private const byte ClockMinutes = 5;
    private const int FiveMinutesInMilliseconds = 300000;
    private const string ClassicFirstSquare = "e1";
    private const string ClassicSecondSquare = "e9";
    private const string QuickSecondSquare = "d7";
    private const int OutsiderAccountId = 9;
    private const byte RoomWallsPerPlayer = 3;
    private const int ThirdAccountId = 7;
    private const int FourthAccountId = 8;
    private const byte LastOfFourPlacement = 4;
    private const byte ThirdPlacement = 3;
    private const byte FirstPlacement = 1;
    private const int ThirtySecondsInMilliseconds = 30000;
    private const int TenSecondsInMilliseconds = 10000;
    private const short FirstMoveNumber = 1;
    private const short SecondMoveNumber = 2;
    private const string ClassicForwardSquare = "e2";
    private const string ClassicTwoAheadSquare = "e3";
    private const string ClassicSecondForwardSquare = "e8";
    private const string QuickFirstPlayerGoalSquare = "d7";
    private const short ThirdMoveNumber = 3;
    private const string CenterGrooveSquare = "e4";
    private const string FirstRowCenterGroove = "e1";
    private const byte SingleWallPerPlayer = 1;
    private const byte ClassicWallsAfterOne = 9;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _endedAt = new DateTime(2026, 11, 5, 18, 7, 0, DateTimeKind.Utc);
    private static readonly MatchEntrant _firstEntrant = new MatchEntrant(FirstAccountId, FirstEloRating);
    private static readonly MatchEntrant _secondEntrant = new MatchEntrant(SecondAccountId, SecondEloRating);
    private static readonly MatchEntrant[] _entrants = { _firstEntrant, _secondEntrant };
    private static readonly BoardCoordinates _centerGroove = BoardLayout.ParseSquare(CenterGrooveSquare);
    private static readonly BoardCoordinates _firstRowCenterGroove = BoardLayout.ParseSquare(FirstRowCenterGroove);
    private static readonly Wall _centerWall = new Wall(_centerGroove, WallOrientation.Horizontal);
    private static readonly Wall _wallAheadOfFirstPawn = new Wall(_firstRowCenterGroove, WallOrientation.Horizontal);
    private static readonly string[] _quickMovesBeforeFirstPlayerGoal =
    {
        "d2", "c7", "d3", "c6", "d4", "c5", "d5", "c4", "d6", "c3",
    };

    [Fact]
    public void StartRanked_TwoEntrants_CreatesOneParticipationEach()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(_entrants.Length, match.Participations.Count);
    }

    [Fact]
    public void StartPrivate_TwoEntrants_CreatesPrivateMatch()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreatePrivateEntrants();

        var match = Match.StartPrivate(format, entrants, _startedAt);

        Assert.Equal(MatchType.Private, match.MatchType);
    }

    [Fact]
    public void StartPrivate_WallsChosenForRoom_GivesThoseWallsToEveryPlayer()
    {
        MatchFormat classicFormat = CreateClassicFormat();
        MatchFormat format = classicFormat with { WallsPerPlayer = RoomWallsPerPlayer };
        MatchEntrant[] entrants = CreatePrivateEntrants();
        byte[] expectedWalls = { RoomWallsPerPlayer, RoomWallsPerPlayer };

        var match = Match.StartPrivate(format, entrants, _startedAt);

        IEnumerable<byte> remainingWalls = match.Participations.Select(participation => participation.RemainingWalls);
        Assert.Equal(expectedWalls, remainingWalls);
    }

    [Fact]
    public void StartPrivate_EntrantsWithoutElo_SavesNoInitialElo()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreatePrivateEntrants();
        short?[] expectedElos = { null, null };

        var match = Match.StartPrivate(format, entrants, _startedAt);

        IEnumerable<short?> initialElos = match.Participations.Select(participation => participation.InitialElo);
        Assert.Equal(expectedElos, initialElos);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesFirstPlayerOnE1()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ClassicFirstSquare, first.CurrentSquare);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesSecondPlayerOnE9()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(ClassicSecondSquare, second.CurrentSquare);
    }

    [Fact]
    public void StartRanked_QuickBoard_PlacesSecondPlayerOnD7()
    {
        GameMode quickMode = GameModeSampleFactory.CreateQuick();
        var format = new MatchFormat(quickMode, ClockMinutes);

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(QuickSecondSquare, second.CurrentSquare);
    }

    [Fact]
    public void StartRanked_SecondEntrant_GetsRedPawn()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(PawnSymbol.Red, second.PawnSymbol);
    }

    [Fact]
    public void StartRanked_ClassicMode_GivesTheModeWalls()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        byte expectedWalls = classicMode.WallsPerPlayer;

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(expectedWalls, first.RemainingWalls);
    }

    [Fact]
    public void StartRanked_FiveMinuteClock_GivesFiveMinutesInMilliseconds()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(FiveMinutesInMilliseconds, first.RemainingClockMilliseconds);
    }

    [Fact]
    public void StartRanked_Entrant_KeepsInitialElo()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(FirstEloRating, first.InitialElo);
    }

    [Fact]
    public void StartRanked_NewMatch_GivesTurnToFirstTurnOrder()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(Match.FirstTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void StartRanked_OneEntrant_ThrowsArgumentException()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = { _firstEntrant };

        Action start = () => Match.StartRanked(format, entrants, _startedAt);

        Assert.Throws<ArgumentException>(start);
    }

    [Fact]
    public void Abandon_FirstPlayerLeaves_FirstPlayerLoses()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ParticipationResult.Lost, first.Result);
    }

    [Fact]
    public void Abandon_FirstPlayerLeaves_RivalWins()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(ParticipationResult.Won, second.Result);
    }

    [Fact]
    public void Abandon_MatchInProgress_FinishesMatch()
    {
        Match match = StartClassicMatch();

        match.Abandon(SecondAccountId, _endedAt);

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void Abandon_MatchInProgress_RecordsAbandonment()
    {
        Match match = StartClassicMatch();

        match.Abandon(SecondAccountId, _endedAt);

        Assert.Equal(MatchEndReason.Abandonment, match.EndReason);
    }

    [Fact]
    public void Abandon_MatchInProgress_RecordsEndTime()
    {
        Match match = StartClassicMatch();

        match.Abandon(SecondAccountId, _endedAt);

        Assert.Equal(_endedAt, match.EndedAt);
    }

    [Fact]
    public void Abandon_FinishedMatch_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        Action abandonAgain = () => match.Abandon(SecondAccountId, _endedAt);

        Assert.Throws<InvalidOperationException>(abandonAgain);
    }

    [Fact]
    public void CanBeAbandonedBy_PlayerOfMatchInProgress_ReturnsTrue()
    {
        Match match = StartClassicMatch();

        bool canAbandon = match.CanBeAbandonedBy(FirstAccountId);

        Assert.True(canAbandon);
    }

    [Fact]
    public void CanBeAbandonedBy_Outsider_ReturnsFalse()
    {
        Match match = StartClassicMatch();

        bool canAbandon = match.CanBeAbandonedBy(OutsiderAccountId);

        Assert.False(canAbandon);
    }

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_KeepsMatchGoing()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Assert.Equal(MatchStatus.InProgress, match.Status);
    }

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_GivesLeaverLastPlace()
    {
        Match match = StartFourPlayerMatch();
        (ParticipationResult? Result, MatchEndReason? EndReason, byte? Placement) expectedOutcome =
            (ParticipationResult.Lost, MatchEndReason.Abandonment, LastOfFourPlacement);

        match.Abandon(FirstAccountId, _endedAt);

        Participation leaver = FindParticipation(match, FirstAccountId);
        Assert.Equal(expectedOutcome, (leaver.Result, leaver.EndReason, leaver.Placement));
    }

    [Fact]
    public void Abandon_FourPlayersSecondLeaver_GivesThirdPlace()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(FirstAccountId, _endedAt);

        match.Abandon(ThirdAccountId, _endedAt);

        Participation secondLeaver = FindParticipation(match, ThirdAccountId);
        Assert.Equal(ThirdPlacement, secondLeaver.Placement);
    }

    [Fact]
    public void Abandon_FourPlayersLeaverHadTurn_PassesTurnToNextPlayer()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Assert.Equal(Match.SecondTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void Abandon_FourPlayersLeaverWithoutTurn_KeepsTurn()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(ThirdAccountId, _endedAt);

        Assert.Equal(Match.FirstTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void Abandon_FourPlayersThreeLeave_LastPlayerWinsFirstPlace()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(FirstAccountId, _endedAt);
        match.Abandon(SecondAccountId, _endedAt);
        (ParticipationResult? Result, byte? Placement, MatchStatus Status) expectedOutcome =
            (ParticipationResult.Won, FirstPlacement, MatchStatus.Finished);

        match.Abandon(ThirdAccountId, _endedAt);

        Participation winner = FindParticipation(match, FourthAccountId);
        Assert.Equal(expectedOutcome, (winner.Result, winner.Placement, match.Status));
    }

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
        PawnMove forwardMove = CreateFirstPlayerForwardMove();

        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ClassicForwardSquare, first.CurrentSquare);
    }

    [Fact]
    public void MovePawn_FirstPlayerForward_PassesTurnToSecondPlayer()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();

        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Equal(Match.SecondTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void MovePawn_SecondPlayerMoves_GivesTurnBackToFirstPlayer()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);
        PawnMove secondMove = CreateSecondPlayerForwardMove();
        DateTime playedAt = _startedAt.AddMilliseconds(TenSecondsInMilliseconds);

        match.MovePawn(secondMove, GameModeSampleFactory.ClassicBoardSize, playedAt);

        Assert.Equal(Match.FirstTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void MovePawn_FirstPlayerForward_RecordsMoveFromStartingSquare()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();

        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Move move = match.Moves.Single();
        Assert.Equal(ClassicFirstSquare, move.FromSquare);
    }

    [Fact]
    public void MovePawn_ThirtySecondsAfterStart_SpendsThoseSecondsFromClock()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        DateTime playedAt = _startedAt.AddMilliseconds(ThirtySecondsInMilliseconds);
        int expectedClock = FiveMinutesInMilliseconds - ThirtySecondsInMilliseconds;

        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, playedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(expectedClock, first.RemainingClockMilliseconds);
    }

    [Fact]
    public void MovePawn_SecondMove_CountsTimeFromPreviousMove()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);
        PawnMove secondMove = CreateSecondPlayerForwardMove();
        DateTime playedAt = _startedAt.AddMilliseconds(TenSecondsInMilliseconds);

        match.MovePawn(secondMove, GameModeSampleFactory.ClassicBoardSize, playedAt);

        Move lastMove = match.Moves.Last();
        Assert.Equal(TenSecondsInMilliseconds, lastMove.TimeSpentMilliseconds);
    }

    [Fact]
    public void MovePawn_SecondPlayerAtStart_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicSecondForwardSquare);
        var pawnMove = new PawnMove(SecondAccountId, FirstMoveNumber, destination);

        Action move = () => match.MovePawn(pawnMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(move);
    }

    [Fact]
    public void MovePawn_UnexpectedMoveNumber_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicForwardSquare);
        var pawnMove = new PawnMove(FirstAccountId, SecondMoveNumber, destination);

        Action move = () => match.MovePawn(pawnMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(move);
    }

    [Fact]
    public void MovePawn_SquareTwoAhead_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicTwoAheadSquare);
        var pawnMove = new PawnMove(FirstAccountId, FirstMoveNumber, destination);

        Action move = () => match.MovePawn(pawnMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(move);
    }

    [Fact]
    public void BuildBoard_AfterFirstMove_PlacesPawnOnDestination()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Board board = match.BuildBoard(GameModeSampleFactory.ClassicBoardSize);

        BoardCoordinates firstPawn = board.GetPawnCoordinates(Match.FirstTurnOrder);
        Assert.Equal(forwardMove.Destination, firstPawn);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_FinishesMatch()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _endedAt);

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_EndsByGoal()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _endedAt);

        Assert.Equal(MatchEndReason.Goal, match.EndReason);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_FirstPlayerWins()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _endedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ParticipationResult.Won, first.Result);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_SecondPlayerLoses()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _endedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(ParticipationResult.Lost, second.Result);
    }

    [Fact]
    public void PlaceWall_FreeGroove_SpendsOneWall()
    {
        Match match = StartClassicMatch();
        WallPlacement placement = CreateFirstPlayerWallPlacement(_centerWall);

        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ClassicWallsAfterOne, first.RemainingWalls);
    }

    [Fact]
    public void PlaceWall_FreeGroove_PassesTurn()
    {
        Match match = StartClassicMatch();
        WallPlacement placement = CreateFirstPlayerWallPlacement(_centerWall);

        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Equal(Match.SecondTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void PlaceWall_FreeGroove_RecordsWallMove()
    {
        Match match = StartClassicMatch();
        WallPlacement placement = CreateFirstPlayerWallPlacement(_centerWall);

        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Move move = match.Moves.Single();
        Wall recordedWall = move.GetWall();
        Assert.Equal(_centerWall, recordedWall);
    }

    [Fact]
    public void PlaceWall_FreeGroove_RecordsGrooveInBoardNotation()
    {
        Match match = StartClassicMatch();
        WallPlacement placement = CreateFirstPlayerWallPlacement(_centerWall);

        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Move move = match.Moves.Single();
        Assert.Equal(CenterGrooveSquare, move.Groove);
    }

    [Fact]
    public void BuildBoard_WallPlaced_ContainsWall()
    {
        Match match = StartClassicMatch();
        PlaceFirstPlayerWall(match, _centerWall);

        Board board = match.BuildBoard(GameModeSampleFactory.ClassicBoardSize);

        bool hasWall = board.HasWall(_centerWall);
        Assert.True(hasWall);
    }

    [Fact]
    public void ListReachableSquares_WallAheadOfPawn_ExcludesSquareBehindWall()
    {
        Match match = StartClassicMatch();
        PlaceFirstPlayerWall(match, _wallAheadOfFirstPawn);
        BoardCoordinates squareBehindWall = BoardLayout.ParseSquare(ClassicForwardSquare);

        IReadOnlyList<BoardCoordinates> reachableSquares =
            match.ListReachableSquares(FirstAccountId, GameModeSampleFactory.ClassicBoardSize);

        Assert.DoesNotContain(squareBehindWall, reachableSquares);
    }

    [Fact]
    public void FindWallRejection_LastWallSpent_RejectsNoWallsLeft()
    {
        Match match = StartClassicMatchWithWalls(SingleWallPerPlayer);
        PlaceFirstPlayerWall(match, _centerWall);
        PawnMove secondMove = CreateSecondPlayerForwardMove();
        match.MovePawn(secondMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);
        var placement = new WallPlacement(FirstAccountId, ThirdMoveNumber, _wallAheadOfFirstPawn);
        var expectedRejection = new WallRejection(WallRejectionReason.NoWallsLeft, BlockedTurnOrder: null);

        WallRejection? rejection = match.FindWallRejection(placement, GameModeSampleFactory.ClassicBoardSize);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void PlaceWall_GrooveTaken_Throws()
    {
        Match match = StartClassicMatch();
        PlaceFirstPlayerWall(match, _centerWall);
        var placement = new WallPlacement(SecondAccountId, SecondMoveNumber, _centerWall);

        Action place = () => match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(place);
    }

    [Fact]
    public void PlaceWall_RivalTurn_Throws()
    {
        Match match = StartClassicMatch();
        var placement = new WallPlacement(SecondAccountId, FirstMoveNumber, _centerWall);

        Action place = () => match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(place);
    }

    private static MatchFormat CreateClassicFormat()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        return new MatchFormat(classicMode, ClockMinutes);
    }

    private static MatchEntrant[] CreatePrivateEntrants()
    {
        MatchEntrant first = CreateUnrankedEntrant(FirstAccountId);
        MatchEntrant second = CreateUnrankedEntrant(SecondAccountId);
        return new MatchEntrant[] { first, second };
    }

    private static MatchEntrant CreateUnrankedEntrant(int accountId)
    {
        return new MatchEntrant(accountId, EloRating: null);
    }

    private static Participation FindParticipation(Match match, int accountId)
    {
        return match.Participations.First(participation => participation.AccountId == accountId);
    }

    private static Match StartClassicMatch()
    {
        MatchFormat format = CreateClassicFormat();
        return Match.StartRanked(format, _entrants, _startedAt);
    }

    private static Match StartFourPlayerMatch()
    {
        GameMode fourPlayerMode = GameModeSampleFactory.CreateFourPlayers();
        var format = new MatchFormat(fourPlayerMode, ClockMinutes);
        MatchEntrant first = CreateUnrankedEntrant(FirstAccountId);
        MatchEntrant second = CreateUnrankedEntrant(SecondAccountId);
        MatchEntrant third = CreateUnrankedEntrant(ThirdAccountId);
        MatchEntrant fourth = CreateUnrankedEntrant(FourthAccountId);
        MatchEntrant[] entrants = { first, second, third, fourth };
        return Match.StartPrivate(format, entrants, _startedAt);
    }

    private static PawnMove CreateFirstPlayerForwardMove()
    {
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicForwardSquare);
        return new PawnMove(FirstAccountId, FirstMoveNumber, destination);
    }

    private static PawnMove CreateSecondPlayerForwardMove()
    {
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicSecondForwardSquare);
        return new PawnMove(SecondAccountId, SecondMoveNumber, destination);
    }

    private static Match PlayQuickMatchBeforeFirstPlayerGoal()
    {
        GameMode quickMode = GameModeSampleFactory.CreateQuick();
        var format = new MatchFormat(quickMode, ClockMinutes);
        var match = Match.StartRanked(format, _entrants, _startedAt);
        for (int moveIndex = 0; moveIndex < _quickMovesBeforeFirstPlayerGoal.Length; moveIndex++)
        {
            int accountId = _entrants[moveIndex % Match.TwoPlayerCount].AccountId;
            short moveNumber = match.GetNextMoveNumber();
            BoardCoordinates destination = BoardLayout.ParseSquare(_quickMovesBeforeFirstPlayerGoal[moveIndex]);
            var pawnMove = new PawnMove(accountId, moveNumber, destination);
            DateTime playedAt = _startedAt.AddSeconds(moveIndex);
            match.MovePawn(pawnMove, GameModeSampleFactory.QuickBoardSize, playedAt);
        }

        return match;
    }

    private static PawnMove CreateFirstPlayerGoalMove(Match match)
    {
        short moveNumber = match.GetNextMoveNumber();
        BoardCoordinates destination = BoardLayout.ParseSquare(QuickFirstPlayerGoalSquare);
        return new PawnMove(FirstAccountId, moveNumber, destination);
    }

    private static WallPlacement CreateFirstPlayerWallPlacement(Wall wall)
    {
        return new WallPlacement(FirstAccountId, FirstMoveNumber, wall);
    }

    private static void PlaceFirstPlayerWall(Match match, Wall wall)
    {
        WallPlacement placement = CreateFirstPlayerWallPlacement(wall);
        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);
    }

    private static Match StartClassicMatchWithWalls(byte wallsPerPlayer)
    {
        MatchFormat classicFormat = CreateClassicFormat();
        MatchFormat format = classicFormat with { WallsPerPlayer = wallsPerPlayer };
        return Match.StartRanked(format, _entrants, _startedAt);
    }
}
