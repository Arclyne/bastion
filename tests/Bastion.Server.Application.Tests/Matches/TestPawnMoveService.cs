using System;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public class TestPawnMoveService
{
    private const int MatchId = 21;
    private const int UnknownMatchId = 99;
    private const int FirstAccountId = 12;
    private const int SecondAccountId = 13;
    private const int OutsiderAccountId = 5;
    private const int TwoPlayers = 2;
    private const string FirstNickname = "prueba_chat_1";
    private const string SecondNickname = "rival_uno";
    private const string OutsiderNickname = "pedro_peon";
    private const string FirstToken = "first-token";
    private const string SecondToken = "second-token";
    private const string OutsiderToken = "outsider-token";
    private const string UnknownToken = "unknown-token";
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;
    private const short FirstMoveNumber = 1;
    private const short SecondMoveNumber = 2;
    private const string ForwardSquare = "e2";
    private const string StartingSquare = "e1";
    private const string TwoAheadSquare = "e3";
    private const string MalformedSquare = "zz";
    private const string SecondPlayerForwardSquare = "e8";

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 12, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _now = new DateTime(2026, 11, 12, 18, 0, 20, DateTimeKind.Utc);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchMoveNotifier _matchMoveNotifier = new FakeMatchMoveNotifier();
    private readonly PawnMoveService _pawnMoveService;

    public TestPawnMoveService()
    {
        _pawnMoveService = new PawnMoveService(_sessionVerifier, _matchRepository, _matchMoveNotifier);
        _sessionVerifier.Add(FirstToken, new ConnectedPlayer(FirstAccountId, FirstNickname));
        _sessionVerifier.Add(SecondToken, new ConnectedPlayer(SecondAccountId, SecondNickname));
        _sessionVerifier.Add(OutsiderToken, new ConnectedPlayer(OutsiderAccountId, OutsiderNickname));
        _matchRepository.MatchesById[MatchId] = StartMatch();
    }

    [Fact]
    public void MovePawn_ValidMove_SavesMatchWithMove()
    {
        _pawnMoveService.MovePawn(FirstToken, CreateMove(FirstMoveNumber, ForwardSquare), _now);

        Assert.Single(_matchRepository.SavedPawnMoves.Single().Moves);
    }

    [Fact]
    public void MovePawn_ValidMove_NotifiesBothPlayers()
    {
        _pawnMoveService.MovePawn(FirstToken, CreateMove(FirstMoveNumber, ForwardSquare), _now);

        Assert.Equal(TwoPlayers, _matchMoveNotifier.NotifiedPawnMoves.Single().RecipientAccountIds.Count);
    }

    [Fact]
    public void MovePawn_ValidMove_NotifiesSquareThePawnLeft()
    {
        _pawnMoveService.MovePawn(FirstToken, CreateMove(FirstMoveNumber, ForwardSquare), _now);

        Assert.Equal(StartingSquare, _matchMoveNotifier.NotifiedPawnMoves.Single().Notice.FromSquare);
    }

    [Fact]
    public void MovePawn_ValidMove_NotifiesSecondPlayerTurn()
    {
        _pawnMoveService.MovePawn(FirstToken, CreateMove(FirstMoveNumber, ForwardSquare), _now);

        Assert.Equal(Match.SecondTurnOrder, _matchMoveNotifier.NotifiedPawnMoves.Single().Notice.NextTurnOrder);
    }

    [Fact]
    public void MovePawn_RivalHasTurn_ThrowsNotYourTurnException()
    {
        ChosenPawnMove move = CreateMove(FirstMoveNumber, SecondPlayerForwardSquare);

        Assert.Throws<NotYourTurnException>(() => _pawnMoveService.MovePawn(SecondToken, move, _now));
    }

    [Fact]
    public void MovePawn_MoveNumberAhead_ThrowsNotYourTurnException()
    {
        ChosenPawnMove move = CreateMove(SecondMoveNumber, ForwardSquare);

        Assert.Throws<NotYourTurnException>(() => _pawnMoveService.MovePawn(FirstToken, move, _now));
    }

    [Fact]
    public void MovePawn_MoveNumberAhead_ReportsExpectedMoveNumber()
    {
        ChosenPawnMove move = CreateMove(SecondMoveNumber, ForwardSquare);

        Exception? failure = Record.Exception(() => _pawnMoveService.MovePawn(FirstToken, move, _now));

        var rejection = failure as NotYourTurnException;
        Assert.Equal(FirstMoveNumber, rejection?.NextMoveNumber);
    }

    [Fact]
    public void MovePawn_UnreachableSquare_ThrowsIllegalMoveException()
    {
        ChosenPawnMove move = CreateMove(FirstMoveNumber, TwoAheadSquare);

        Assert.Throws<IllegalMoveException>(() => _pawnMoveService.MovePawn(FirstToken, move, _now));
    }

    [Fact]
    public void MovePawn_MalformedSquare_ThrowsIllegalMoveException()
    {
        ChosenPawnMove move = CreateMove(FirstMoveNumber, MalformedSquare);

        Assert.Throws<IllegalMoveException>(() => _pawnMoveService.MovePawn(FirstToken, move, _now));
    }

    [Fact]
    public void MovePawn_UnreachableSquare_SavesNothing()
    {
        ChosenPawnMove move = CreateMove(FirstMoveNumber, TwoAheadSquare);

        Record.Exception(() => _pawnMoveService.MovePawn(FirstToken, move, _now));

        Assert.Empty(_matchRepository.SavedPawnMoves);
    }

    [Fact]
    public void MovePawn_Outsider_ThrowsMatchNotJoinedException()
    {
        ChosenPawnMove move = CreateMove(FirstMoveNumber, ForwardSquare);

        Assert.Throws<MatchNotJoinedException>(() => _pawnMoveService.MovePawn(OutsiderToken, move, _now));
    }

    [Fact]
    public void MovePawn_UnknownMatch_ThrowsMatchNotJoinedException()
    {
        var move = new ChosenPawnMove(UnknownMatchId, FirstMoveNumber, ForwardSquare);

        Assert.Throws<MatchNotJoinedException>(() => _pawnMoveService.MovePawn(FirstToken, move, _now));
    }

    [Fact]
    public void MovePawn_UnknownSession_ThrowsInvalidSessionException()
    {
        ChosenPawnMove move = CreateMove(FirstMoveNumber, ForwardSquare);

        Assert.Throws<InvalidSessionException>(() => _pawnMoveService.MovePawn(UnknownToken, move, _now));
    }

    private static ChosenPawnMove CreateMove(short moveNumber, string destinationSquare)
    {
        return new ChosenPawnMove(MatchId, moveNumber, destinationSquare);
    }

    private static Match StartMatch()
    {
        var entrants = new MatchEntrant[]
        {
            new MatchEntrant(FirstAccountId, StartingElo),
            new MatchEntrant(SecondAccountId, StartingElo),
        };
        var format = new MatchFormat(GameModeSampleFactory.CreateClassic(), ClockMinutes);
        return Match.StartRanked(format, entrants, _startedAt);
    }
}
