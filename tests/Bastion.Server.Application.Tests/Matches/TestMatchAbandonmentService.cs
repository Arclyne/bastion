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

public sealed class TestMatchAbandonmentService
{
    private const int MatchId = 16;
    private const int UnknownMatchId = 99;
    private const int AbandoningAccountId = 12;
    private const int RivalAccountId = 13;
    private const int OutsiderAccountId = 5;
    private const string AbandoningNickname = "prueba_chat_1";
    private const string OutsiderNickname = "pedro_peon";
    private const string AbandoningToken = "abandoning-token";
    private const string OutsiderToken = "outsider-token";
    private const string UnknownToken = "unknown-token";
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;
    private const int FourPlayerMatchId = 17;
    private const int ThirdAccountId = 14;
    private const int FourthAccountId = 15;
    private const string RivalNickname = "rival_uno";
    private const string RivalToken = "rival-token";
    private const int ThreeActivePlayers = 3;
    private const short LeaverFinalElo = 984;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 7, 0, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _abandoningPlayer = new ConnectedPlayer(AbandoningAccountId, AbandoningNickname);
    private static readonly ConnectedPlayer _rival = new ConnectedPlayer(RivalAccountId, RivalNickname);
    private static readonly ConnectedPlayer _outsider = new ConnectedPlayer(OutsiderAccountId, OutsiderNickname);
    private static readonly MatchEntrant _abandoningEntrant = new MatchEntrant(AbandoningAccountId, StartingElo);
    private static readonly MatchEntrant _rivalEntrant = new MatchEntrant(RivalAccountId, StartingElo);
    private static readonly MatchEntrant[] _rankedEntrants = { _abandoningEntrant, _rivalEntrant };

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchEndNotifier _matchEndNotifier = new FakeMatchEndNotifier();
    private readonly MatchAbandonmentService _abandonmentService;

    public TestMatchAbandonmentService()
    {
        var matchCloser = new MatchCloser(_matchRepository, _matchEndNotifier);
        _abandonmentService = new MatchAbandonmentService(_sessionVerifier, _matchRepository, matchCloser);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_TellsOthersThreePlayersGoOn()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddFourPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        NotifiedParticipationEnd notified = _matchEndNotifier.NotifiedParticipationEnds.Single();
        Assert.Equal(ThreeActivePlayers, notified.Notice.ActivePlayerCount);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_NotifiesAbandonmentAsEndReason()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddFourPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        NotifiedParticipationEnd notified = _matchEndNotifier.NotifiedParticipationEnds.Single();
        Assert.Equal(MatchEndReason.Abandonment, notified.Notice.EndReason);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_SavesMatchWithoutClosingIt()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddFourPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        Assert.Single(_matchRepository.MatchesSavedWithDeparture);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_NotifiesLeaverAndRemainingPlayers()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddFourPlayerMatch();
        int[] expectedRecipients = { AbandoningAccountId, RivalAccountId, ThirdAccountId, FourthAccountId };

        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        NotifiedParticipationEnd notified = _matchEndNotifier.NotifiedParticipationEnds.Single();
        Assert.Equal(expectedRecipients, notified.RecipientAccountIds);
    }

    [Fact]
    public void AbandonMatch_FourPlayersSecondLeaver_SkipsEarlierLeaver()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        _sessionVerifier.Add(RivalToken, _rival);
        AddFourPlayerMatch();
        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);
        int[] expectedRecipients = { RivalAccountId, ThirdAccountId, FourthAccountId };

        _abandonmentService.AbandonMatch(RivalToken, FourPlayerMatchId, _now);

        NotifiedParticipationEnd secondNotice = _matchEndNotifier.NotifiedParticipationEnds.Last();
        Assert.Equal(expectedRecipients, secondNotice.RecipientAccountIds);
    }

    [Fact]
    public void AbandonMatch_Player_SavesFinishedMatch()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        MatchClosure savedClosure = _matchRepository.ClosuresSavedWithoutMove.Single();
        Assert.Equal(MatchStatus.Finished, savedClosure.Match.Status);
    }

    [Fact]
    public void AbandonMatch_Player_AbandoningPlayerLoses()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        MatchClosure savedClosure = _matchRepository.ClosuresSavedWithoutMove.Single();
        Participation abandoning = savedClosure.Match.GetParticipation(AbandoningAccountId);
        Assert.Equal(ParticipationResult.Lost, abandoning.Result);
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesLeaverAndRival()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();
        int[] expectedRecipients = { AbandoningAccountId, RivalAccountId };

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(expectedRecipients, notified.RecipientAccountIds);
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesAbandoningPlayerAsDecidingPlayer()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(Match.FirstTurnOrder, notified.Notice.DecidingTurnOrder);
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesAbandonmentAsEndReason()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(MatchEndReason.Abandonment, notified.Notice.EndReason);
    }

    [Fact]
    public void AbandonMatch_ClosureNotSaved_NotifiesNobody()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();
        _matchRepository.ClosureSaveFailure = new MatchRepositoryException();
        Action abandon = () => _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Record.Exception(abandon);

        Assert.Empty(_matchEndNotifier.NotifiedMatchEnds);
    }

    [Fact]
    public void AbandonMatch_Outsider_ThrowsMatchNotJoinedException()
    {
        _sessionVerifier.Add(OutsiderToken, _outsider);
        AddTwoPlayerMatch();

        Action abandon = () => _abandonmentService.AbandonMatch(OutsiderToken, MatchId, _now);

        Assert.Throws<MatchNotJoinedException>(abandon);
    }

    [Fact]
    public void AbandonMatch_UnknownMatch_ThrowsMatchNotJoinedException()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);

        Action abandon = () => _abandonmentService.AbandonMatch(AbandoningToken, UnknownMatchId, _now);

        Assert.Throws<MatchNotJoinedException>(abandon);
    }

    [Fact]
    public void AbandonMatch_FinishedMatch_ThrowsMatchNotJoinedException()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();
        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Action abandonAgain = () => _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Assert.Throws<MatchNotJoinedException>(abandonAgain);
    }

    [Fact]
    public void AbandonMatch_UnknownSession_ThrowsInvalidSessionException()
    {
        Action abandon = () => _abandonmentService.AbandonMatch(UnknownToken, MatchId, _now);

        Assert.Throws<InvalidSessionException>(abandon);
    }

    [Fact]
    public void PreviewAbandonment_RankedMatch_PredictsLeaverFinalElo()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        AbandonmentPreview preview = _abandonmentService.PreviewAbandonment(AbandoningToken, MatchId, _now);

        Assert.Equal(LeaverFinalElo, preview.FinalElo);
    }

    [Fact]
    public void PreviewAbandonment_RankedMatch_SavesNothing()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        _abandonmentService.PreviewAbandonment(AbandoningToken, MatchId, _now);

        Assert.Empty(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void PreviewAbandonment_RankedMatch_NotifiesNobody()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();

        _abandonmentService.PreviewAbandonment(AbandoningToken, MatchId, _now);

        Assert.Empty(_matchEndNotifier.NotifiedMatchEnds);
    }

    [Fact]
    public void PreviewAbandonment_ThenAbandon_StillAbandonsTheMatch()
    {
        AddAbandoningPlayerWithTwoPlayerMatch();
        _abandonmentService.PreviewAbandonment(AbandoningToken, MatchId, _now);

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Assert.Single(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void PreviewAbandonment_Outsider_ThrowsMatchNotJoinedException()
    {
        _sessionVerifier.Add(OutsiderToken, _outsider);
        AddTwoPlayerMatch();

        Func<AbandonmentPreview> preview = () => _abandonmentService.PreviewAbandonment(OutsiderToken, MatchId, _now);

        Assert.Throws<MatchNotJoinedException>(preview);
    }

    private void AddFourPlayerMatch()
    {
        GameMode fourPlayerMode = GameModeSampleFactory.CreateFourPlayers();
        var format = new MatchFormat(fourPlayerMode, ClockMinutes);
        MatchEntrant abandoning = CreateUnrankedEntrant(AbandoningAccountId);
        MatchEntrant rival = CreateUnrankedEntrant(RivalAccountId);
        MatchEntrant third = CreateUnrankedEntrant(ThirdAccountId);
        MatchEntrant fourth = CreateUnrankedEntrant(FourthAccountId);
        MatchEntrant[] entrants = { abandoning, rival, third, fourth };
        var match = Match.StartPrivate(format, entrants, _startedAt);
        _matchRepository.MatchesById[FourPlayerMatchId] = match;
    }

    private static MatchEntrant CreateUnrankedEntrant(int accountId)
    {
        return new MatchEntrant(accountId, EloRating: null);
    }

    private void AddAbandoningPlayerWithTwoPlayerMatch()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddTwoPlayerMatch();
    }

    private void AddTwoPlayerMatch()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        var match = Match.StartRanked(format, _rankedEntrants, _startedAt);
        _matchRepository.MatchesById[MatchId] = match;
    }
}
