using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Tests.GameModes;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public sealed class TestMatchDraw
{
    private const int FirstAccountId = 3;
    private const int SecondAccountId = 6;
    private const int ThirdAccountId = 7;
    private const int FourthAccountId = 8;
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _offeredAt = new DateTime(2026, 11, 19, 18, 1, 0, DateTimeKind.Utc);
    private static readonly DateTime _answeredAt = new DateTime(2026, 11, 19, 18, 1, 20, DateTimeKind.Utc);
    private static readonly string[] _firstPlayerSquares = ["e2", "e3", "e4", "e5", "e6"];
    private static readonly string[] _secondPlayerSquares = ["d9", "c9", "b9", "a9"];

    [Fact]
    public void OfferDraw_TwoPlayers_LeavesAPendingOffer()
    {
        Match match = StartTwoPlayerMatch();

        match.OfferDraw(FirstAccountId, _offeredAt);

        Assert.True(match.HasPendingDrawOfferFrom(FirstAccountId));
    }

    [Fact]
    public void OfferDraw_TwoPlayers_ReturnsOffered()
    {
        Match match = StartTwoPlayerMatch();

        DrawOfferOutcome outcome = match.OfferDraw(FirstAccountId, _offeredAt);

        Assert.Equal(DrawOfferOutcome.Offered, outcome);
    }

    [Fact]
    public void FindDrawOfferRejection_FourPlayers_RejectsNotTwoPlayers()
    {
        Match match = StartFourPlayerMatch();

        DrawOfferRejection rejection = match.FindDrawOfferRejection(FirstAccountId);

        Assert.Equal(DrawOfferRejectionReason.NotTwoPlayers, rejection.Reason);
    }

    [Fact]
    public void FindDrawOfferRejection_AlreadyOffered_RejectsAlreadyOffered()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        DrawOfferRejection rejection = match.FindDrawOfferRejection(FirstAccountId);

        Assert.Equal(DrawOfferRejectionReason.AlreadyOffered, rejection.Reason);
    }

    [Fact]
    public void FindDrawOfferRejection_JustRejected_SaysFiveMovesLeft()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);
        match.RejectDraw(SecondAccountId, _answeredAt);

        DrawOfferRejection rejection = match.FindDrawOfferRejection(FirstAccountId);

        Assert.Equal(MatchDrawPolicy.OwnMovesBetweenOffers, rejection.MovesLeft);
    }

    [Fact]
    public void FindDrawOfferRejection_FiveOwnMovesAfterRejection_AllowsTheOffer()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);
        match.RejectDraw(SecondAccountId, _answeredAt);
        PlayFiveFirstPlayerMoves(match);

        DrawOfferRejection rejection = match.FindDrawOfferRejection(FirstAccountId);

        Assert.Equal(DrawOfferRejectionReason.None, rejection.Reason);
    }

    [Fact]
    public void OfferDraw_RivalAlreadyOffered_AgreesTheDraw()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        DrawOfferOutcome outcome = match.OfferDraw(SecondAccountId, _answeredAt);

        Assert.Equal(DrawOfferOutcome.Agreed, outcome);
    }

    [Fact]
    public void AcceptDraw_PendingOffer_EndsTheMatchWithDraw()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        match.AcceptDraw(SecondAccountId, _answeredAt);

        Assert.Equal(MatchEndReason.Draw, match.EndReason);
    }

    [Fact]
    public void AcceptDraw_PendingOffer_GivesBothPlayersADraw()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        match.AcceptDraw(SecondAccountId, _answeredAt);

        Assert.All(match.Participations, participation => Assert.Equal(ParticipationResult.Draw, participation.Result));
    }

    [Fact]
    public void AcceptDraw_OwnOffer_ThrowsInvalidOperationException()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        Action acceptDraw = () => match.AcceptDraw(FirstAccountId, _answeredAt);

        Assert.Throws<InvalidOperationException>(acceptDraw);
    }

    [Fact]
    public void RejectDraw_PendingOffer_LeavesNoPendingOffer()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        match.RejectDraw(SecondAccountId, _answeredAt);

        Assert.False(match.HasPendingDrawOfferFrom(FirstAccountId));
    }

    [Fact]
    public void MovePawn_ReceiverPlays_ExpiresTheOffer()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);
        PlaySquare(match, FirstAccountId, "e2");

        PlaySquare(match, SecondAccountId, "d9");

        Assert.Equal(DrawOfferStatus.Expired, match.DrawOffers.Single().Status);
    }

    [Fact]
    public void MovePawn_OffererPlays_KeepsTheOffer()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        PlaySquare(match, FirstAccountId, "e2");

        Assert.True(match.HasPendingDrawOfferFrom(FirstAccountId));
    }

    [Fact]
    public void Abandon_PendingOffer_ExpiresTheOffer()
    {
        Match match = StartTwoPlayerMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        match.Abandon(SecondAccountId, _answeredAt);

        Assert.Equal(DrawOfferStatus.Expired, match.DrawOffers.Single().Status);
    }

    private static void PlayFiveFirstPlayerMoves(Match match)
    {
        for (int moveIndex = 0; moveIndex < _firstPlayerSquares.Length; moveIndex++)
        {
            PlaySquare(match, FirstAccountId, _firstPlayerSquares[moveIndex]);
            if (moveIndex < _secondPlayerSquares.Length)
            {
                PlaySquare(match, SecondAccountId, _secondPlayerSquares[moveIndex]);
            }
        }
    }

    private static void PlaySquare(Match match, int accountId, string square)
    {
        short moveNumber = match.GetNextMoveNumber();
        BoardCoordinates destination = BoardLayout.ParseSquare(square);
        var pawnMove = new PawnMove(accountId, moveNumber, destination);
        match.MovePawn(pawnMove, GameModeSampleFactory.ClassicBoardSize, _offeredAt);
    }

    private static Match StartTwoPlayerMatch()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(FirstAccountId, StartingElo),
            new MatchEntrant(SecondAccountId, StartingElo),
        ];
        return Match.StartRanked(format, entrants, _startedAt);
    }

    private static Match StartFourPlayerMatch()
    {
        GameMode fourPlayerMode = GameModeSampleFactory.CreateFourPlayers();
        var format = new MatchFormat(fourPlayerMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(FirstAccountId, EloRating: null),
            new MatchEntrant(SecondAccountId, EloRating: null),
            new MatchEntrant(ThirdAccountId, EloRating: null),
            new MatchEntrant(FourthAccountId, EloRating: null),
        ];
        return Match.StartPrivate(format, entrants, _startedAt);
    }
}
