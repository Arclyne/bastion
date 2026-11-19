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

public sealed class TestMatchDrawService
{
    private const int MatchId = 51;
    private const int FirstAccountId = 12;
    private const int SecondAccountId = 13;
    private const int OutsiderAccountId = 5;
    private const string FirstToken = "first-token";
    private const string SecondToken = "second-token";
    private const string OutsiderToken = "outsider-token";
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _offeredAt = new DateTime(2026, 11, 19, 18, 1, 0, DateTimeKind.Utc);
    private static readonly DateTime _answeredAt = new DateTime(2026, 11, 19, 18, 1, 20, DateTimeKind.Utc);
    private static readonly DrawResponse _acceptance = new DrawResponse(MatchId, IsAccepted: true);
    private static readonly DrawResponse _rejection = new DrawResponse(MatchId, IsAccepted: false);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchEndNotifier _matchEndNotifier = new FakeMatchEndNotifier();
    private readonly FakeMatchDrawNotifier _drawNotifier = new FakeMatchDrawNotifier();
    private readonly MatchDrawService _drawService;

    public TestMatchDrawService()
    {
        var matchCloser = new MatchCloser(_matchRepository, _matchEndNotifier);
        var followUp = new MatchDrawFollowUp(matchCloser, _drawNotifier);
        _drawService = new MatchDrawService(_sessionVerifier, _matchRepository, followUp);
        _sessionVerifier.Add(FirstToken, new ConnectedPlayer(FirstAccountId, "marta_muros"));
        _sessionVerifier.Add(SecondToken, new ConnectedPlayer(SecondAccountId, "pedro_peon"));
        _sessionVerifier.Add(OutsiderToken, new ConnectedPlayer(OutsiderAccountId, "nico_nuevo"));
    }

    [Fact]
    public void OfferDraw_TwoPlayers_SavesTheOffer()
    {
        AddMatch();

        _drawService.OfferDraw(FirstToken, MatchId, _offeredAt);

        Assert.Single(_matchRepository.MatchesSavedWithDrawOffer);
    }

    [Fact]
    public void OfferDraw_TwoPlayers_TellsOnlyTheRival()
    {
        AddMatch();
        int[] expectedRecipients = { SecondAccountId };

        _drawService.OfferDraw(FirstToken, MatchId, _offeredAt);

        Assert.Equal(expectedRecipients, _drawNotifier.OfferRecipients);
    }

    [Fact]
    public void OfferDraw_TwoPlayers_SaysWhoOffered()
    {
        AddMatch();

        _drawService.OfferDraw(FirstToken, MatchId, _offeredAt);

        Assert.Equal(Match.FirstTurnOrder, _drawNotifier.OffererTurnOrders.Single());
    }

    [Fact]
    public void OfferDraw_AlreadyOffered_ThrowsDrawOfferRejectedException()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        Action offerDraw = () => _drawService.OfferDraw(FirstToken, MatchId, _answeredAt);

        Assert.Throws<DrawOfferRejectedException>(offerDraw);
    }

    [Fact]
    public void OfferDraw_RivalAlreadyOffered_ClosesTheMatch()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        _drawService.OfferDraw(SecondToken, MatchId, _answeredAt);

        Assert.Single(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void OfferDraw_Outsider_ThrowsMatchNotJoinedException()
    {
        AddMatch();

        Action offerDraw = () => _drawService.OfferDraw(OutsiderToken, MatchId, _offeredAt);

        Assert.Throws<MatchNotJoinedException>(offerDraw);
    }

    [Fact]
    public void RespondToDraw_Accepted_AnnouncesADraw()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        _drawService.RespondToDraw(SecondToken, _acceptance, _answeredAt);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(MatchEndReason.Draw, notified.Notice.EndReason);
    }

    [Fact]
    public void RespondToDraw_Accepted_SavesTheElo()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        _drawService.RespondToDraw(SecondToken, _acceptance, _answeredAt);

        MatchClosure closure = _matchRepository.ClosuresSavedWithoutMove.Single();
        Assert.Equal(StartingElo, closure.Match.GetParticipation(FirstAccountId).FinalElo);
    }

    [Fact]
    public void RespondToDraw_Rejected_TellsTheOfferer()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);
        int[] expectedRecipients = { FirstAccountId };

        _drawService.RespondToDraw(SecondToken, _rejection, _answeredAt);

        Assert.Equal(expectedRecipients, _drawNotifier.RejectionRecipients);
    }

    [Fact]
    public void RespondToDraw_Rejected_KeepsTheMatchGoing()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        _drawService.RespondToDraw(SecondToken, _rejection, _answeredAt);

        Assert.False(match.IsFinished());
    }

    [Fact]
    public void RespondToDraw_NoOffer_ThrowsDrawNotPendingException()
    {
        AddMatch();

        Action respondToDraw = () => _drawService.RespondToDraw(SecondToken, _acceptance, _answeredAt);

        Assert.Throws<DrawNotPendingException>(respondToDraw);
    }

    [Fact]
    public void RespondToDraw_OwnOffer_ThrowsDrawNotPendingException()
    {
        Match match = AddMatch();
        match.OfferDraw(FirstAccountId, _offeredAt);

        Action respondToDraw = () => _drawService.RespondToDraw(FirstToken, _acceptance, _answeredAt);

        Assert.Throws<DrawNotPendingException>(respondToDraw);
    }

    private Match AddMatch()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(FirstAccountId, StartingElo),
            new MatchEntrant(SecondAccountId, StartingElo),
        ];
        var match = Match.StartRanked(format, entrants, _startedAt);
        _matchRepository.MatchesById[MatchId] = match;
        return match;
    }
}
