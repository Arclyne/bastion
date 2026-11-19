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
    private const short SecondMoveNumber = 2;
    private const string ClassicForwardSquare = "e2";
    private const string ClassicTwoAheadSquare = "e3";
    private const string ClassicSecondForwardSquare = "e8";
    private const string QuickFirstPlayerGoalSquare = "d7";
    private const short ThirdMoveNumber = 3;
    private const string CenterGrooveSquare = "e4";
    private const string FirstRowCenterGroove = "e1";
    private const byte SingleWallPerPlayer = 1;
    private const byte ClassicWallsAfterOne = GameModeSampleFactory.ClassicWallsPerPlayer - 1;
    private const byte SecondPlacement = 2;
    private const short RankedWinnerFinalElo = 1019;
    private const short RankedLeaverFinalElo = 1036;
    private const int ThreeActivePlayers = 3;
    private const string FourPlayerGoalSquare = "e9";
    private const int NoActivePlayers = 0;
    private const int NoClockLeft = 0;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _endedAt = new DateTime(2026, 11, 5, 18, 7, 0, DateTimeKind.Utc);
    private static readonly DateTime _goalReachedAt = new DateTime(2026, 11, 5, 18, 1, 0, DateTimeKind.Utc);
    private static readonly DateTime _turnOpensAt = _startedAt + MatchClockPolicy.OpeningDelay;
    private static readonly DateTime _clockRunsOutAt = _turnOpensAt.AddMilliseconds(FiveMinutesInMilliseconds);
    private static readonly DateTime _disconnectedAt = new DateTime(2026, 11, 5, 18, 1, 0, DateTimeKind.Utc);
    private static readonly DateTime _halfMinuteAway = _disconnectedAt.AddSeconds(30);
    private static readonly DateTime _fullDelayAway = _disconnectedAt + MatchConnectionPolicy.ReturnDelay;
    private static readonly DateTime _restartedAt = new DateTime(2026, 11, 5, 18, 2, 0, DateTimeKind.Utc);
    private static readonly MatchEntrant _firstEntrant = new MatchEntrant(FirstAccountId, FirstEloRating);
    private static readonly MatchEntrant _secondEntrant = new MatchEntrant(SecondAccountId, SecondEloRating);
    private static readonly MatchEntrant[] _entrants = { _firstEntrant, _secondEntrant };
    private static readonly BoardCoordinates _centerGroove = BoardLayout.ParseSquare(CenterGrooveSquare);
    private static readonly BoardCoordinates _firstRowCenterGroove = BoardLayout.ParseSquare(FirstRowCenterGroove);
    private static readonly Wall _centerWall = new Wall(_centerGroove, WallOrientation.Horizontal);
    private static readonly Wall _wallAheadOfFirstPawn = new Wall(_firstRowCenterGroove, WallOrientation.Horizontal);
    private static readonly string[] _fourPlayerMovesBeforeFirstGoal =
    {
        "e2", "f9", "a4", "i4", "e3", "e9", "a5", "i5", "e4", "f9", "a4", "i4", "e5", "e9",
        "a5", "i5", "e6", "f9", "a4", "i4", "e7", "e9", "a5", "i5", "e8", "f9", "a4", "i4",
    };

    private static readonly string[] _quickMovesBeforeFirstPlayerGoal =
    {
        "d2", "c7", "d3", "c6", "d4", "c5", "d5", "c4", "d6", "c3",
    };

    [Fact]
    public void StartRanked_TwoEntrants_CreatesOneParticipationEach()
    {
        MatchFormat format = CreateClassicFormat();
        int[] expectedAccountIds = { FirstAccountId, SecondAccountId };

        var match = Match.StartRanked(format, _entrants, _startedAt);

        IEnumerable<int> accountIds = match.Participations.Select(participation => participation.AccountId);
        Assert.Equal(expectedAccountIds, accountIds);
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
        MatchFormat format = CreateClassicFormatWithWalls(RoomWallsPerPlayer);
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

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(ClassicFirstSquare, first.CurrentSquare);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesSecondPlayerOnE9()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.Equal(ClassicSecondSquare, second.CurrentSquare);
    }

    [Fact]
    public void StartRanked_QuickBoard_PlacesSecondPlayerOnD7()
    {
        MatchFormat format = CreateQuickFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.Equal(QuickSecondSquare, second.CurrentSquare);
    }

    [Fact]
    public void StartRanked_SecondEntrant_GetsRedPawn()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.Equal(PawnSymbol.Red, second.PawnSymbol);
    }

    [Fact]
    public void StartRanked_ClassicMode_GivesTheModeWalls()
    {
        MatchFormat format = CreateClassicFormat();
        byte expectedWalls = GameModeSampleFactory.ClassicWallsPerPlayer;

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(expectedWalls, first.RemainingWalls);
    }

    [Fact]
    public void StartRanked_FiveMinuteClock_GivesFiveMinutesInMilliseconds()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(FiveMinutesInMilliseconds, first.RemainingClockMilliseconds);
    }

    [Fact]
    public void StartRanked_Entrant_KeepsInitialElo()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = match.GetParticipation(FirstAccountId);
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

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(ParticipationResult.Lost, first.Result);
    }

    [Fact]
    public void Abandon_FirstPlayerLeaves_RivalWins()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Participation second = match.GetParticipation(SecondAccountId);
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

        match.Abandon(FirstAccountId, _endedAt);

        Participation leaver = match.GetParticipation(FirstAccountId);
        Assert.Equal(LastOfFourPlacement, leaver.Placement);
    }

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_RecordsLeaverLossByAbandonment()
    {
        Match match = StartFourPlayerMatch();
        (ParticipationResult? Result, MatchEndReason? EndReason) expectedOutcome =
            (ParticipationResult.Lost, MatchEndReason.Abandonment);

        match.Abandon(FirstAccountId, _endedAt);

        Participation leaver = match.GetParticipation(FirstAccountId);
        Assert.Equal(expectedOutcome, (leaver.Result, leaver.EndReason));
    }

    [Fact]
    public void Abandon_FourPlayersSecondLeaver_GivesThirdPlace()
    {
        Match match = StartFourPlayerMatchAfterFirstLeaver();

        match.Abandon(ThirdAccountId, _endedAt);

        Participation secondLeaver = match.GetParticipation(ThirdAccountId);
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
        Match match = StartFourPlayerMatchAfterFirstLeaver();
        match.Abandon(SecondAccountId, _endedAt);
        (ParticipationResult? Result, byte? Placement) expectedOutcome = (ParticipationResult.Won, FirstPlacement);

        match.Abandon(ThirdAccountId, _endedAt);

        Participation winner = match.GetParticipation(FourthAccountId);
        Assert.Equal(expectedOutcome, (winner.Result, winner.Placement));
    }

    [Fact]
    public void Abandon_FourPlayersThreeLeave_FinishesMatch()
    {
        Match match = StartFourPlayerMatchAfterFirstLeaver();
        match.Abandon(SecondAccountId, _endedAt);

        match.Abandon(ThirdAccountId, _endedAt);

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void GetNextMoveNumber_NoMoves_ReturnsOne()
    {
        Match match = StartClassicMatch();

        short nextMoveNumber = match.GetNextMoveNumber();

        Assert.Equal(Match.FirstMoveNumber, nextMoveNumber);
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

        Participation first = match.GetParticipation(FirstAccountId);
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
        MoveFirstPlayerForward(match);
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

        Move playedMove = match.Moves.Single();
        Assert.Equal(ClassicFirstSquare, playedMove.FromSquare);
    }

    [Fact]
    public void MovePawn_ThirtySecondsAfterOpening_SpendsThoseSecondsFromClock()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        DateTime playedAt = _turnOpensAt.AddMilliseconds(ThirtySecondsInMilliseconds);
        int expectedClock = FiveMinutesInMilliseconds - ThirtySecondsInMilliseconds;

        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, playedAt);

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(expectedClock, first.RemainingClockMilliseconds);
    }

    [Fact]
    public void MovePawn_SecondMove_CountsTimeFromPreviousMove()
    {
        Match match = StartClassicMatch();
        MoveFirstPlayerForward(match);
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
        var pawnMove = new PawnMove(SecondAccountId, Match.FirstMoveNumber, destination);

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
        var pawnMove = new PawnMove(FirstAccountId, Match.FirstMoveNumber, destination);

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

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _goalReachedAt);

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_EndsByGoal()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _goalReachedAt);

        Assert.Equal(MatchEndReason.Goal, match.EndReason);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_FirstPlayerWins()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _goalReachedAt);

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(ParticipationResult.Won, first.Result);
    }

    [Fact]
    public void MovePawn_FirstPlayerReachesGoal_SecondPlayerLoses()
    {
        Match match = PlayQuickMatchBeforeFirstPlayerGoal();
        PawnMove goalMove = CreateFirstPlayerGoalMove(match);

        match.MovePawn(goalMove, GameModeSampleFactory.QuickBoardSize, _goalReachedAt);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.Equal(ParticipationResult.Lost, second.Result);
    }

    [Fact]
    public void PlaceWall_FreeGroove_SpendsOneWall()
    {
        Match match = StartClassicMatch();
        WallPlacement placement = CreateFirstPlayerWallPlacement(_centerWall);

        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Participation first = match.GetParticipation(FirstAccountId);
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

        Move playedMove = match.Moves.Single();
        Wall recordedWall = playedMove.GetWall();
        Assert.Equal(_centerWall, recordedWall);
    }

    [Fact]
    public void PlaceWall_FreeGroove_RecordsGrooveInBoardNotation()
    {
        Match match = StartClassicMatch();
        WallPlacement placement = CreateFirstPlayerWallPlacement(_centerWall);

        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Move playedMove = match.Moves.Single();
        Assert.Equal(CenterGrooveSquare, playedMove.Groove);
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

        IReadOnlyList<BoardCoordinates> reachableSquares = match.ListReachableSquares(
            FirstAccountId,
            GameModeSampleFactory.ClassicBoardSize);

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

        WallRejection rejection = match.FindWallRejection(placement, GameModeSampleFactory.ClassicBoardSize);

        Assert.Equal(expectedRejection, rejection);
    }

    [Fact]
    public void PlaceWall_GrooveTaken_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        PlaceFirstPlayerWall(match, _centerWall);
        var placement = new WallPlacement(SecondAccountId, SecondMoveNumber, _centerWall);

        Action place = () => match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(place);
    }

    [Fact]
    public void PlaceWall_RivalTurn_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        var placement = new WallPlacement(SecondAccountId, Match.FirstMoveNumber, _centerWall);

        Action place = () => match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);

        Assert.Throws<InvalidOperationException>(place);
    }

    [Fact]
    public void MovePawn_FourPlayersFirstReachesGoal_KeepsMatchGoing()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();

        Assert.False(match.IsFinished());
    }

    [Fact]
    public void MovePawn_FourPlayersFirstReachesGoal_GivesFirstPlace()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();

        Participation arriving = match.GetParticipation(FirstAccountId);
        Assert.Equal(FirstPlacement, arriving.Placement);
    }

    [Fact]
    public void MovePawn_FourPlayersFirstReachesGoal_RecordsGoalAsEndReason()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();

        Participation arriving = match.GetParticipation(FirstAccountId);
        Assert.Equal(MatchEndReason.Goal, arriving.EndReason);
    }

    [Fact]
    public void MovePawn_FourPlayersFirstReachesGoal_PassesTurnToSecondPlayer()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();

        Assert.True(match.IsTurnOf(SecondAccountId));
    }

    [Fact]
    public void MovePawn_FourPlayersFirstReachesGoal_LeavesThreeActivePlayers()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();

        Assert.Equal(ThreeActivePlayers, match.CountActivePlayers());
    }

    [Fact]
    public void Abandon_FourPlayersAfterFirstGoal_GivesLeaverLastPlace()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();

        match.Abandon(SecondAccountId, _endedAt);

        Participation leaver = match.GetParticipation(SecondAccountId);
        Assert.Equal(LastOfFourPlacement, leaver.Placement);
    }

    [Fact]
    public void Abandon_FourPlayersAfterFirstGoalTwoLeave_GivesLastStandingSecondPlace()
    {
        Match match = PlayFourPlayerMatchUntilFirstGoal();
        match.Abandon(SecondAccountId, _endedAt);
        (ParticipationResult? Result, byte? Placement) expectedOutcome = (ParticipationResult.Lost, SecondPlacement);

        match.Abandon(ThirdAccountId, _endedAt);

        Participation lastStanding = match.GetParticipation(FourthAccountId);
        Assert.Equal(expectedOutcome, (lastStanding.Result, lastStanding.Placement));
    }

    [Fact]
    public void CanBeRated_RankedMatchAbandoned_ReturnsTrue()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        Assert.True(match.CanBeRated());
    }

    [Fact]
    public void CanBeRated_MatchInProgress_ReturnsFalse()
    {
        Match match = StartClassicMatch();

        Assert.False(match.CanBeRated());
    }

    [Fact]
    public void CanBeRated_PrivateMatchAbandoned_ReturnsFalse()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreatePrivateEntrants();
        var match = Match.StartPrivate(format, entrants, _startedAt);
        match.Abandon(FirstAccountId, _endedAt);

        Assert.False(match.CanBeRated());
    }

    [Fact]
    public void Rate_RankedMatchAbandoned_RaisesWinnerElo()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        match.Rate();

        Participation winner = match.GetParticipation(SecondAccountId);
        Assert.Equal(RankedWinnerFinalElo, winner.FinalElo);
    }

    [Fact]
    public void Rate_RankedMatchAbandoned_LowersLeaverElo()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        match.Rate();

        Participation leaver = match.GetParticipation(FirstAccountId);
        Assert.Equal(RankedLeaverFinalElo, leaver.FinalElo);
    }

    [Fact]
    public void Rate_MatchInProgress_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();

        Action rate = match.Rate;

        Assert.Throws<InvalidOperationException>(rate);
    }

    [Fact]
    public void HasAbandoned_TwoPlayerLeaver_ReturnsTrue()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        Assert.True(match.HasAbandoned(FirstAccountId));
    }

    [Fact]
    public void HasAbandoned_TwoPlayerWinnerByAbandonment_ReturnsFalse()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        Assert.False(match.HasAbandoned(SecondAccountId));
    }

    [Fact]
    public void HasAbandoned_FourPlayerLeaver_ReturnsTrue()
    {
        Match match = StartFourPlayerMatchAfterFirstLeaver();

        Assert.True(match.HasAbandoned(FirstAccountId));
    }

    [Fact]
    public void PreviewAbandonment_RankedMatch_PredictsLeaverFinalElo()
    {
        Match match = StartClassicMatch();

        AbandonmentPreview preview = match.PreviewAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(RankedLeaverFinalElo, preview.FinalElo);
    }

    [Fact]
    public void PreviewAbandonment_RankedMatch_KeepsMatchInProgress()
    {
        Match match = StartClassicMatch();

        match.PreviewAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(MatchStatus.InProgress, match.Status);
    }

    [Fact]
    public void PreviewAbandonment_RankedMatch_KeepsLeaverParticipationOpen()
    {
        Match match = StartClassicMatch();

        match.PreviewAbandonment(FirstAccountId, _endedAt);

        Participation leaver = match.GetParticipation(FirstAccountId);
        Assert.Null(leaver.Result);
    }

    [Fact]
    public void PreviewAbandonment_TwoPlayers_PredictsNoActivePlayers()
    {
        Match match = StartClassicMatch();

        AbandonmentPreview preview = match.PreviewAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(NoActivePlayers, preview.ActivePlayerCount);
    }

    [Fact]
    public void PreviewAbandonment_PrivateMatch_PredictsNoFinalElo()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreatePrivateEntrants();
        var match = Match.StartPrivate(format, entrants, _startedAt);

        AbandonmentPreview preview = match.PreviewAbandonment(FirstAccountId, _endedAt);

        Assert.Null(preview.FinalElo);
    }

    [Fact]
    public void PreviewAbandonment_FourPlayers_PredictsLastPlace()
    {
        Match match = StartFourPlayerMatch();

        AbandonmentPreview preview = match.PreviewAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(LastOfFourPlacement, preview.Placement);
    }

    [Fact]
    public void PreviewAbandonment_FourPlayers_PredictsThreeActivePlayers()
    {
        Match match = StartFourPlayerMatch();

        AbandonmentPreview preview = match.PreviewAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(ThreeActivePlayers, preview.ActivePlayerCount);
    }

    [Fact]
    public void PreviewAbandonment_Outsider_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();

        Func<AbandonmentPreview> preview = () => match.PreviewAbandonment(OutsiderAccountId, _endedAt);

        Assert.Throws<InvalidOperationException>(preview);
    }

    [Fact]
    public void StartRanked_NewMatch_OpensFirstTurnAfterOpeningDelay()
    {
        Match match = StartClassicMatch();

        Assert.Equal(_turnOpensAt, match.TurnStartedAt);
    }

    [Fact]
    public void MovePawn_FirstPlayerMoves_OpensNextTurnAtMoveTime()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        DateTime playedAt = _turnOpensAt.AddMilliseconds(TenSecondsInMilliseconds);

        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, playedAt);

        Assert.Equal(playedAt, match.TurnStartedAt);
    }

    [Fact]
    public void HasClockRunOut_BeforeClockEnds_ReturnsFalse()
    {
        Match match = StartClassicMatch();
        DateTime beforeEnd = _clockRunsOutAt.AddMilliseconds(-TenSecondsInMilliseconds);

        Assert.False(match.HasClockRunOut(beforeEnd));
    }

    [Fact]
    public void HasClockRunOut_WhenClockEnds_ReturnsTrue()
    {
        Match match = StartClassicMatch();

        Assert.True(match.HasClockRunOut(_clockRunsOutAt));
    }

    [Fact]
    public void ExpireClock_TwoPlayers_RivalWins()
    {
        Match match = StartClassicMatch();

        match.ExpireClock(_clockRunsOutAt);

        Participation rival = match.GetParticipation(SecondAccountId);
        Assert.Equal(ParticipationResult.Won, rival.Result);
    }

    [Fact]
    public void ExpireClock_TwoPlayers_EndsByTimeout()
    {
        Match match = StartClassicMatch();

        match.ExpireClock(_clockRunsOutAt);

        Assert.Equal(MatchEndReason.Timeout, match.EndReason);
    }

    [Fact]
    public void ExpireClock_LateCheck_EndsWhenTheClockRanOut()
    {
        Match match = StartClassicMatch();
        DateTime checkedAt = _clockRunsOutAt.AddMilliseconds(TenSecondsInMilliseconds);

        match.ExpireClock(checkedAt);

        Assert.Equal(_clockRunsOutAt, match.EndedAt);
    }

    [Fact]
    public void ExpireClock_TwoPlayers_LeavesTimedOutClockAtZero()
    {
        Match match = StartClassicMatch();

        match.ExpireClock(_clockRunsOutAt);

        Participation timedOut = match.GetParticipation(FirstAccountId);
        Assert.Equal(NoClockLeft, timedOut.RemainingClockMilliseconds);
    }

    [Fact]
    public void ExpireClock_ClockLeft_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();

        Action expire = () => match.ExpireClock(_turnOpensAt);

        Assert.Throws<InvalidOperationException>(expire);
    }

    [Fact]
    public void ExpireClock_FourPlayers_GivesTimedOutPlayerLastPlace()
    {
        Match match = StartFourPlayerMatch();

        match.ExpireClock(_clockRunsOutAt);

        Participation timedOut = match.GetParticipation(FirstAccountId);
        Assert.Equal(LastOfFourPlacement, timedOut.Placement);
    }

    [Fact]
    public void ExpireClock_FourPlayers_OpensNextTurnWhenTheClockRanOut()
    {
        Match match = StartFourPlayerMatch();

        match.ExpireClock(_clockRunsOutAt);

        Assert.Equal((Match.SecondTurnOrder, _clockRunsOutAt), (match.CurrentTurnOrder, match.TurnStartedAt));
    }

    [Fact]
    public void MovePawn_ClockRanOut_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        PawnMove forwardMove = CreateFirstPlayerForwardMove();

        Action movePawn = () => match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _clockRunsOutAt);

        Assert.Throws<InvalidOperationException>(movePawn);
    }

    [Fact]
    public void Disconnect_ActivePlayer_MarksParticipationDisconnected()
    {
        Match match = StartClassicMatch();

        match.Disconnect(SecondAccountId, _disconnectedAt);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.False(second.IsConnected);
    }

    [Fact]
    public void Disconnect_ActivePlayer_RecordsWhenItHappened()
    {
        Match match = StartClassicMatch();

        match.Disconnect(SecondAccountId, _disconnectedAt);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.Equal(_disconnectedAt, second.DisconnectedAt);
    }

    [Fact]
    public void Disconnect_Outsider_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();

        Action disconnect = () => match.Disconnect(OutsiderAccountId, _disconnectedAt);

        Assert.Throws<InvalidOperationException>(disconnect);
    }

    [Fact]
    public void Reconnect_DisconnectedPlayer_MarksParticipationConnected()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        match.Reconnect(SecondAccountId);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.True(second.IsConnected);
    }

    [Fact]
    public void HasAbsentPlayerToTakeOut_AwayHalfAMinute_ReturnsFalse()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        bool hasAbsentPlayer = match.HasAbsentPlayerToTakeOut(_halfMinuteAway);

        Assert.False(hasAbsentPlayer);
    }

    [Fact]
    public void HasAbsentPlayerToTakeOut_AwayTheWholeDelay_ReturnsTrue()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        bool hasAbsentPlayer = match.HasAbsentPlayerToTakeOut(_fullDelayAway);

        Assert.True(hasAbsentPlayer);
    }

    [Fact]
    public void HasAbsentPlayerToTakeOut_CameBack_ReturnsFalse()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);
        match.Reconnect(SecondAccountId);

        bool hasAbsentPlayer = match.HasAbsentPlayerToTakeOut(_fullDelayAway);

        Assert.False(hasAbsentPlayer);
    }

    [Fact]
    public void HasAbsentPlayerToTakeOut_EveryPlayerAway_ReturnsFalse()
    {
        Match match = StartClassicMatch();
        match.Disconnect(FirstAccountId, _disconnectedAt);
        match.Disconnect(SecondAccountId, _disconnectedAt);

        bool hasAbsentPlayer = match.HasAbsentPlayerToTakeOut(_fullDelayAway);

        Assert.False(hasAbsentPlayer);
    }

    [Fact]
    public void TakeOutAbsentPlayer_TwoPlayers_GivesTheConnectedPlayerTheWin()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        match.TakeOutAbsentPlayer(_fullDelayAway);

        Participation first = match.GetParticipation(FirstAccountId);
        Assert.Equal(ParticipationResult.Won, first.Result);
    }

    [Fact]
    public void TakeOutAbsentPlayer_TwoPlayers_EndsByAbandonment()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        match.TakeOutAbsentPlayer(_fullDelayAway);

        Assert.Equal(MatchEndReason.Abandonment, match.EndReason);
    }

    [Fact]
    public void TakeOutAbsentPlayer_CheckedLate_EndsWhenTheDelayRanOut()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        match.TakeOutAbsentPlayer(_restartedAt);

        Assert.Equal(_fullDelayAway, match.EndedAt);
    }

    [Fact]
    public void TakeOutAbsentPlayer_TwoPlayers_ReturnsTheAbsentAccount()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        int absentAccountId = match.TakeOutAbsentPlayer(_fullDelayAway);

        Assert.Equal(SecondAccountId, absentAccountId);
    }

    [Fact]
    public void TakeOutAbsentPlayer_FourPlayers_GivesTheWorstFreePlace()
    {
        Match match = StartFourPlayerMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        match.TakeOutAbsentPlayer(_fullDelayAway);

        Participation second = match.GetParticipation(SecondAccountId);
        Assert.Equal(LastOfFourPlacement, second.Placement);
    }

    [Fact]
    public void TakeOutAbsentPlayer_TooSoon_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        Action takeOut = () => match.TakeOutAbsentPlayer(_halfMinuteAway);

        Assert.Throws<InvalidOperationException>(takeOut);
    }

    [Fact]
    public void RecoverAfterRestart_MatchInProgress_MarksEveryPlayerDisconnected()
    {
        Match match = StartClassicMatch();

        match.RecoverAfterRestart(_restartedAt);

        Assert.All(match.Participations, participation => Assert.False(participation.IsConnected));
    }

    [Fact]
    public void RecoverAfterRestart_TurnStartedBefore_StartsTheTurnAtTheRestart()
    {
        Match match = StartClassicMatch();

        match.RecoverAfterRestart(_restartedAt);

        Assert.Equal(_restartedAt, match.TurnStartedAt);
    }

    [Fact]
    public void RecoverAfterRestart_LongDowntime_LeavesTheClockRunning()
    {
        Match match = StartClassicMatch();

        match.RecoverAfterRestart(_endedAt);

        Assert.False(match.HasClockRunOut(_endedAt));
    }

    private static Match PlayFourPlayerMatchUntilFirstGoal()
    {
        Match match = StartFourPlayerMatch();
        int[] accountIdsInTurnOrder = { FirstAccountId, SecondAccountId, ThirdAccountId, FourthAccountId };
        for (int moveIndex = 0; moveIndex < _fourPlayerMovesBeforeFirstGoal.Length; moveIndex++)
        {
            int accountId = accountIdsInTurnOrder[moveIndex % accountIdsInTurnOrder.Length];
            PlaySquare(match, accountId, _fourPlayerMovesBeforeFirstGoal[moveIndex]);
        }

        PlaySquare(match, FirstAccountId, FourPlayerGoalSquare);
        return match;
    }

    private static void PlaySquare(Match match, int accountId, string square)
    {
        short moveNumber = match.GetNextMoveNumber();
        BoardCoordinates destination = BoardLayout.ParseSquare(square);
        var pawnMove = new PawnMove(accountId, moveNumber, destination);
        match.MovePawn(pawnMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);
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

    private static Match StartClassicMatch()
    {
        MatchFormat format = CreateClassicFormat();
        return Match.StartRanked(format, _entrants, _startedAt);
    }

    private static MatchFormat CreateClassicFormat()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        return new MatchFormat(classicMode, ClockMinutes);
    }

    private static Match StartFourPlayerMatchAfterFirstLeaver()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(FirstAccountId, _endedAt);
        return match;
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

    private static void MoveFirstPlayerForward(Match match)
    {
        PawnMove forwardMove = CreateFirstPlayerForwardMove();
        match.MovePawn(forwardMove, GameModeSampleFactory.ClassicBoardSize, _startedAt);
    }

    private static PawnMove CreateFirstPlayerForwardMove()
    {
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicForwardSquare);
        return new PawnMove(FirstAccountId, Match.FirstMoveNumber, destination);
    }

    private static PawnMove CreateSecondPlayerForwardMove()
    {
        BoardCoordinates destination = BoardLayout.ParseSquare(ClassicSecondForwardSquare);
        return new PawnMove(SecondAccountId, SecondMoveNumber, destination);
    }

    private static Match PlayQuickMatchBeforeFirstPlayerGoal()
    {
        MatchFormat format = CreateQuickFormat();
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

    private static MatchFormat CreateQuickFormat()
    {
        GameMode quickMode = GameModeSampleFactory.CreateQuick();
        return new MatchFormat(quickMode, ClockMinutes);
    }

    private static PawnMove CreateFirstPlayerGoalMove(Match match)
    {
        short moveNumber = match.GetNextMoveNumber();
        BoardCoordinates destination = BoardLayout.ParseSquare(QuickFirstPlayerGoalSquare);
        return new PawnMove(FirstAccountId, moveNumber, destination);
    }

    private static void PlaceFirstPlayerWall(Match match, Wall wall)
    {
        WallPlacement placement = CreateFirstPlayerWallPlacement(wall);
        match.PlaceWall(placement, GameModeSampleFactory.ClassicBoardSize, _startedAt);
    }

    private static WallPlacement CreateFirstPlayerWallPlacement(Wall wall)
    {
        return new WallPlacement(FirstAccountId, Match.FirstMoveNumber, wall);
    }

    private static Match StartClassicMatchWithWalls(byte wallsPerPlayer)
    {
        MatchFormat format = CreateClassicFormatWithWalls(wallsPerPlayer);
        return Match.StartRanked(format, _entrants, _startedAt);
    }

    private static MatchFormat CreateClassicFormatWithWalls(byte wallsPerPlayer)
    {
        MatchFormat classicFormat = CreateClassicFormat();
        MatchFormat format = classicFormat with { WallsPerPlayer = wallsPerPlayer };
        return format;
    }
}
