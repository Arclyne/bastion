using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public sealed class TestMatchMoveService
{
    private const int MatchId = 21;
    private const int UnknownMatchId = 99;
    private const int FirstAccountId = 12;
    private const int SecondAccountId = 13;
    private const int OutsiderAccountId = 5;
    private const string FirstNickname = "prueba_chat_1";
    private const string SecondNickname = "rival_uno";
    private const string OutsiderNickname = "pedro_peon";
    private const string FirstToken = "first-token";
    private const string SecondToken = "second-token";
    private const string OutsiderToken = "outsider-token";
    private const string UnknownToken = "unknown-token";
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;
    private const short SecondMoveNumber = 2;
    private const short ThirdMoveNumber = 3;
    private const string ForwardSquare = "e2";
    private const string StartingSquare = "e1";
    private const string TwoAheadSquare = "e3";
    private const string MalformedSquare = "zz";
    private const string SecondPlayerForwardSquare = "e8";
    private const string CenterGroove = "e4";
    private const string FirstRowCenterGroove = "e1";
    private const string LastColumnGroove = "i1";
    private const byte ClassicWallsAfterOne = 9;
    private const byte SingleWallPerPlayer = 1;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 12, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _now = new DateTime(2026, 11, 12, 18, 0, 20, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _firstPlayer = new ConnectedPlayer(FirstAccountId, FirstNickname);
    private static readonly ConnectedPlayer _secondPlayer = new ConnectedPlayer(SecondAccountId, SecondNickname);
    private static readonly ConnectedPlayer _outsider = new ConnectedPlayer(OutsiderAccountId, OutsiderNickname);
    private static readonly MatchEntrant _firstEntrant = new MatchEntrant(FirstAccountId, StartingElo);
    private static readonly MatchEntrant _secondEntrant = new MatchEntrant(SecondAccountId, StartingElo);
    private static readonly MatchEntrant[] _entrants = { _firstEntrant, _secondEntrant };

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchMoveNotifier _matchMoveNotifier = new FakeMatchMoveNotifier();
    private readonly MatchMoveService _matchMoveService;

    public TestMatchMoveService()
    {
        _matchMoveService = new MatchMoveService(_sessionVerifier, _matchRepository, _matchMoveNotifier);
    }

    [Fact]
    public void MovePawn_ValidMove_SavesMatchWithMove()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, ForwardSquare);

        _matchMoveService.MovePawn(FirstToken, move, _now);

        Match savedMatch = _matchRepository.SavedMoves.Single();
        Assert.Single(savedMatch.Moves);
    }

    [Fact]
    public void MovePawn_ValidMove_NotifiesBothPlayers()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, ForwardSquare);

        _matchMoveService.MovePawn(FirstToken, move, _now);

        NotifiedPawnMove notified = _matchMoveNotifier.NotifiedPawnMoves.Single();
        Assert.Equal(Match.TwoPlayerCount, notified.RecipientAccountIds.Count);
    }

    [Fact]
    public void MovePawn_ValidMove_NotifiesSquareThePawnLeft()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, ForwardSquare);

        _matchMoveService.MovePawn(FirstToken, move, _now);

        NotifiedPawnMove notified = _matchMoveNotifier.NotifiedPawnMoves.Single();
        Assert.Equal(StartingSquare, notified.Notice.FromSquare);
    }

    [Fact]
    public void MovePawn_ValidMove_NotifiesSecondPlayerTurn()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, ForwardSquare);

        _matchMoveService.MovePawn(FirstToken, move, _now);

        NotifiedPawnMove notified = _matchMoveNotifier.NotifiedPawnMoves.Single();
        Assert.Equal(Match.SecondTurnOrder, notified.Notice.NextTurnOrder);
    }

    [Fact]
    public void MovePawn_RivalHasTurn_ThrowsNotYourTurnException()
    {
        _sessionVerifier.Add(SecondToken, _secondPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, SecondPlayerForwardSquare);
        Action movePawn = () => _matchMoveService.MovePawn(SecondToken, move, _now);

        Assert.Throws<NotYourTurnException>(movePawn);
    }

    [Fact]
    public void MovePawn_MoveNumberAhead_ThrowsNotYourTurnException()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(SecondMoveNumber, ForwardSquare);
        Action movePawn = () => _matchMoveService.MovePawn(FirstToken, move, _now);

        Assert.Throws<NotYourTurnException>(movePawn);
    }

    [Fact]
    public void MovePawn_MoveNumberAhead_ReportsExpectedMoveNumber()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(SecondMoveNumber, ForwardSquare);
        Action movePawn = () => _matchMoveService.MovePawn(FirstToken, move, _now);

        Exception? exception = Record.Exception(movePawn);

        var rejection = exception as NotYourTurnException;
        Assert.Equal(Match.FirstMoveNumber, rejection?.NextMoveNumber);
    }

    [Fact]
    public void MovePawn_UnreachableSquare_ThrowsIllegalMoveException()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, TwoAheadSquare);
        Action movePawn = () => _matchMoveService.MovePawn(FirstToken, move, _now);

        Assert.Throws<IllegalMoveException>(movePawn);
    }

    [Fact]
    public void MovePawn_MalformedSquare_ThrowsIllegalMoveException()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, MalformedSquare);
        Action movePawn = () => _matchMoveService.MovePawn(FirstToken, move, _now);

        Assert.Throws<IllegalMoveException>(movePawn);
    }

    [Fact]
    public void MovePawn_UnreachableSquare_SavesNothing()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, TwoAheadSquare);
        Action movePawn = () => _matchMoveService.MovePawn(FirstToken, move, _now);

        Record.Exception(movePawn);

        Assert.Empty(_matchRepository.SavedMoves);
    }

    [Fact]
    public void MovePawn_Outsider_ThrowsMatchNotJoinedException()
    {
        _sessionVerifier.Add(OutsiderToken, _outsider);
        AddClassicMatch();
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, ForwardSquare);
        Action movePawn = () => _matchMoveService.MovePawn(OutsiderToken, move, _now);

        Assert.Throws<MatchNotJoinedException>(movePawn);
    }

    [Fact]
    public void MovePawn_UnknownMatch_ThrowsMatchNotJoinedException()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        var move = new ChosenPawnMove(UnknownMatchId, Match.FirstMoveNumber, ForwardSquare);
        Action movePawn = () => _matchMoveService.MovePawn(FirstToken, move, _now);

        Assert.Throws<MatchNotJoinedException>(movePawn);
    }

    [Fact]
    public void MovePawn_UnknownSession_ThrowsInvalidSessionException()
    {
        ChosenPawnMove move = CreateMove(Match.FirstMoveNumber, ForwardSquare);
        Action movePawn = () => _matchMoveService.MovePawn(UnknownToken, move, _now);

        Assert.Throws<InvalidSessionException>(movePawn);
    }

    [Fact]
    public void PlaceWall_FreeGroove_SavesMatchWithWallMove()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, CenterGroove);

        _matchMoveService.PlaceWall(FirstToken, wall, _now);

        Match savedMatch = _matchRepository.SavedMoves.Single();
        Move savedMove = savedMatch.Moves.Single();
        Assert.Equal(MoveType.Wall, savedMove.MoveType);
    }

    [Fact]
    public void PlaceWall_FreeGroove_NotifiesBothPlayers()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, CenterGroove);

        _matchMoveService.PlaceWall(FirstToken, wall, _now);

        NotifiedWallPlacement notified = _matchMoveNotifier.NotifiedWallPlacements.Single();
        Assert.Equal(Match.TwoPlayerCount, notified.RecipientAccountIds.Count);
    }

    [Fact]
    public void PlaceWall_FreeGroove_NotifiesWallsLeft()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, CenterGroove);

        _matchMoveService.PlaceWall(FirstToken, wall, _now);

        NotifiedWallPlacement notified = _matchMoveNotifier.NotifiedWallPlacements.Single();
        Assert.Equal(ClassicWallsAfterOne, notified.Notice.RemainingWalls);
    }

    [Fact]
    public void PlaceWall_FreeGroove_NotifiesSecondPlayerTurn()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, CenterGroove);

        _matchMoveService.PlaceWall(FirstToken, wall, _now);

        NotifiedWallPlacement notified = _matchMoveNotifier.NotifiedWallPlacements.Single();
        Assert.Equal(Match.SecondTurnOrder, notified.Notice.NextTurnOrder);
    }

    [Fact]
    public void PlaceWall_RivalHasTurn_ThrowsNotYourTurnException()
    {
        _sessionVerifier.Add(SecondToken, _secondPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, CenterGroove);
        Action placeWall = () => _matchMoveService.PlaceWall(SecondToken, wall, _now);

        Assert.Throws<NotYourTurnException>(placeWall);
    }

    [Fact]
    public void PlaceWall_GrooveTaken_RejectsWithGrooveTaken()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        _sessionVerifier.Add(SecondToken, _secondPlayer);
        AddClassicMatch();
        PlaceFirstPlayerCenterWall();
        ChosenWall wall = CreateWall(SecondMoveNumber, CenterGroove);
        Action placeWall = () => _matchMoveService.PlaceWall(SecondToken, wall, _now);

        Exception? exception = Record.Exception(placeWall);

        var rejection = exception as IllegalWallException;
        Assert.Equal(WallRejectionReason.GrooveTaken, rejection?.Reason);
    }

    [Fact]
    public void PlaceWall_GrooveOnLastColumn_RejectsWithOutOfBoard()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, LastColumnGroove);
        Action placeWall = () => _matchMoveService.PlaceWall(FirstToken, wall, _now);

        Exception? exception = Record.Exception(placeWall);

        var rejection = exception as IllegalWallException;
        Assert.Equal(WallRejectionReason.OutOfBoard, rejection?.Reason);
    }

    [Fact]
    public void PlaceWall_MalformedGroove_RejectsWithOutOfBoard()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        AddClassicMatch();
        ChosenWall wall = CreateWall(Match.FirstMoveNumber, MalformedSquare);
        Action placeWall = () => _matchMoveService.PlaceWall(FirstToken, wall, _now);

        Exception? exception = Record.Exception(placeWall);

        var rejection = exception as IllegalWallException;
        Assert.Equal(WallRejectionReason.OutOfBoard, rejection?.Reason);
    }

    [Fact]
    public void PlaceWall_NoWallsLeft_RejectsWithNoWallsLeft()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        _sessionVerifier.Add(SecondToken, _secondPlayer);
        MatchFormat classicFormat = CreateClassicFormat();
        MatchFormat singleWallFormat = classicFormat with { WallsPerPlayer = SingleWallPerPlayer };
        AddMatch(singleWallFormat);
        PlaceFirstPlayerCenterWall();
        ChosenPawnMove secondMove = CreateMove(SecondMoveNumber, SecondPlayerForwardSquare);
        _matchMoveService.MovePawn(SecondToken, secondMove, _now);
        ChosenWall wall = CreateWall(ThirdMoveNumber, FirstRowCenterGroove);
        Action placeWall = () => _matchMoveService.PlaceWall(FirstToken, wall, _now);

        Exception? exception = Record.Exception(placeWall);

        var rejection = exception as IllegalWallException;
        Assert.Equal(WallRejectionReason.NoWallsLeft, rejection?.Reason);
    }

    [Fact]
    public void PlaceWall_GrooveTaken_SavesOnlyFirstWall()
    {
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        _sessionVerifier.Add(SecondToken, _secondPlayer);
        AddClassicMatch();
        PlaceFirstPlayerCenterWall();
        ChosenWall wall = CreateWall(SecondMoveNumber, CenterGroove);
        Action placeWall = () => _matchMoveService.PlaceWall(SecondToken, wall, _now);

        Record.Exception(placeWall);

        Assert.Single(_matchRepository.SavedMoves);
    }

    private void AddClassicMatch()
    {
        MatchFormat classicFormat = CreateClassicFormat();
        AddMatch(classicFormat);
    }

    private static MatchFormat CreateClassicFormat()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        return new MatchFormat(classicMode, ClockMinutes);
    }

    private void AddMatch(MatchFormat format)
    {
        var match = Match.StartRanked(format, _entrants, _startedAt);
        _matchRepository.MatchesById[MatchId] = match;
    }

    private static ChosenPawnMove CreateMove(short moveNumber, string destinationSquare)
    {
        return new ChosenPawnMove(MatchId, moveNumber, destinationSquare);
    }

    private static ChosenWall CreateWall(short moveNumber, string groove)
    {
        return new ChosenWall
        {
            MatchId = MatchId,
            MoveNumber = moveNumber,
            Groove = groove,
            Orientation = WallOrientation.Horizontal,
        };
    }

    private void PlaceFirstPlayerCenterWall()
    {
        ChosenWall centerWall = CreateWall(Match.FirstMoveNumber, CenterGroove);
        _matchMoveService.PlaceWall(FirstToken, centerWall, _now);
    }
}
