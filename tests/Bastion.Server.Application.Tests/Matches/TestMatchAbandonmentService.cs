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
    private const int RemainingAfterFirstLeaver = 3;

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
        _abandonmentService = new MatchAbandonmentService(_sessionVerifier, _matchRepository, _matchEndNotifier);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_TellsOthersMatchGoesOn()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddFourPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        NotifiedMatchAbandonment notified = _matchEndNotifier.NotifiedAbandonments.Single();
        Assert.False(notified.Notice.IsMatchOver);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_NotifiesRemainingPlayers()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddFourPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        NotifiedMatchAbandonment notified = _matchEndNotifier.NotifiedAbandonments.Single();
        Assert.Equal(RemainingAfterFirstLeaver, notified.RecipientAccountIds.Count);
    }

    [Fact]
    public void AbandonMatch_FourPlayersSecondLeaver_SkipsEarlierLeaver()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        _sessionVerifier.Add(RivalToken, _rival);
        AddFourPlayerMatch();
        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);
        int[] expectedRecipients = { ThirdAccountId, FourthAccountId };

        _abandonmentService.AbandonMatch(RivalToken, FourPlayerMatchId, _now);

        NotifiedMatchAbandonment secondNotice = _matchEndNotifier.NotifiedAbandonments.Last();
        Assert.Equal(expectedRecipients, secondNotice.RecipientAccountIds);
    }

    [Fact]
    public void AbandonMatch_Player_SavesFinishedMatch()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Match savedMatch = _matchRepository.SavedAbandonments.Single();
        Assert.Equal(MatchStatus.Finished, savedMatch.Status);
    }

    [Fact]
    public void AbandonMatch_Player_AbandoningPlayerLoses()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Match savedMatch = _matchRepository.SavedAbandonments.Single();
        Participation abandoning = savedMatch.Participations
            .Single(participation => participation.AccountId == AbandoningAccountId);
        Assert.Equal(ParticipationResult.Lost, abandoning.Result);
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesOnlyTheRival()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddTwoPlayerMatch();
        int[] expectedRecipients = { RivalAccountId };

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        NotifiedMatchAbandonment notified = _matchEndNotifier.NotifiedAbandonments.Single();
        Assert.Equal(expectedRecipients, notified.RecipientAccountIds);
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesAbandoningNickname()
    {
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddTwoPlayerMatch();

        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        NotifiedMatchAbandonment notified = _matchEndNotifier.NotifiedAbandonments.Single();
        Assert.Equal(AbandoningNickname, notified.Notice.AbandoningNickname);
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
        _sessionVerifier.Add(AbandoningToken, _abandoningPlayer);
        AddTwoPlayerMatch();
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

    private static MatchEntrant CreateUnrankedEntrant(int accountId)
    {
        return new MatchEntrant(accountId, EloRating: null);
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

    private void AddTwoPlayerMatch()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        var match = Match.StartRanked(format, _rankedEntrants, _startedAt);
        _matchRepository.MatchesById[MatchId] = match;
    }
}
