using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public class TestMatchAbandonmentService
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
    private const byte ClassicGameModeId = 1;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicPlayerCount = 2;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte ClockMinutes = 5;
    private const int FourPlayerMatchId = 17;
    private const int ThirdAccountId = 14;
    private const int FourthAccountId = 15;
    private const string RivalNickname = "rival_uno";
    private const string RivalToken = "rival-token";
    private const byte FourPlayers = 4;
    private const int RemainingAfterFirstLeaver = 3;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 7, 0, DateTimeKind.Utc);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchStore _matchStore = new FakeMatchStore();
    private readonly FakeMatchEndNotifier _matchEndNotifier = new FakeMatchEndNotifier();
    private readonly MatchAbandonmentService _abandonmentService;

    public TestMatchAbandonmentService()
    {
        _abandonmentService = new MatchAbandonmentService(_sessionVerifier, _matchStore, _matchEndNotifier);
        _sessionVerifier.Add(AbandoningToken, new ConnectedPlayer(AbandoningAccountId, AbandoningNickname));
        _sessionVerifier.Add(OutsiderToken, new ConnectedPlayer(OutsiderAccountId, OutsiderNickname));
        _sessionVerifier.Add(RivalToken, new ConnectedPlayer(RivalAccountId, RivalNickname));
        _matchStore.MatchesById[MatchId] = StartMatch();
        _matchStore.MatchesById[FourPlayerMatchId] = StartFourPlayerMatch();
    }

    [Fact]
    public void AbandonMatch_FourPlayers_TellsOthersMatchGoesOn()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        Assert.False(_matchEndNotifier.NotifiedAbandonments.Single().Notice.IsMatchOver);
    }

    [Fact]
    public void AbandonMatch_FourPlayers_NotifiesRemainingPlayers()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        NotifiedMatchAbandonment notified = _matchEndNotifier.NotifiedAbandonments.Single();
        Assert.Equal(RemainingAfterFirstLeaver, notified.RecipientAccountIds.Count);
    }

    [Fact]
    public void AbandonMatch_FourPlayersSecondLeaver_SkipsEarlierLeaver()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, FourPlayerMatchId, _now);

        _abandonmentService.AbandonMatch(RivalToken, FourPlayerMatchId, _now);

        Assert.DoesNotContain(AbandoningAccountId, _matchEndNotifier.NotifiedAbandonments.Last().RecipientAccountIds);
    }

    [Fact]
    public void AbandonMatch_Player_SavesFinishedMatch()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Assert.Equal(MatchStatus.Finished, _matchStore.SavedAbandonments.Single().Status);
    }

    [Fact]
    public void AbandonMatch_Player_AbandoningPlayerLoses()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Participation abandoning = _matchStore.SavedAbandonments.Single().Participations
            .Single(participation => participation.AccountId == AbandoningAccountId);
        Assert.Equal(ParticipationResult.Lost, abandoning.Result);
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesOnlyTheRival()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Assert.Equal(RivalAccountId, _matchEndNotifier.NotifiedAbandonments.Single().RecipientAccountIds.Single());
    }

    [Fact]
    public void AbandonMatch_Player_NotifiesAbandoningNickname()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Assert.Equal(AbandoningNickname, _matchEndNotifier.NotifiedAbandonments.Single().Notice.AbandoningNickname);
    }

    [Fact]
    public void AbandonMatch_Outsider_ThrowsMatchNotJoinedException()
    {
        Assert.Throws<MatchNotJoinedException>(() => _abandonmentService.AbandonMatch(OutsiderToken, MatchId, _now));
    }

    [Fact]
    public void AbandonMatch_UnknownMatch_ThrowsMatchNotJoinedException()
    {
        Assert.Throws<MatchNotJoinedException>(
            () => _abandonmentService.AbandonMatch(AbandoningToken, UnknownMatchId, _now));
    }

    [Fact]
    public void AbandonMatch_FinishedMatch_ThrowsMatchNotJoinedException()
    {
        _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now);

        Assert.Throws<MatchNotJoinedException>(() => _abandonmentService.AbandonMatch(AbandoningToken, MatchId, _now));
    }

    [Fact]
    public void AbandonMatch_UnknownSession_ThrowsInvalidSessionException()
    {
        Assert.Throws<InvalidSessionException>(() => _abandonmentService.AbandonMatch(UnknownToken, MatchId, _now));
    }

    private static Match StartFourPlayerMatch()
    {
        var gameMode = new GameMode
        {
            GameModeId = ClassicGameModeId,
            Code = GameModeCode.FourPlayers,
            BoardSize = ClassicBoardSize,
            PlayerCount = FourPlayers,
            WallsPerPlayer = ClassicWallsPerPlayer,
            IsActive = true,
        };
        var entrants = new MatchEntrant[]
        {
            new MatchEntrant(AbandoningAccountId, null),
            new MatchEntrant(RivalAccountId, null),
            new MatchEntrant(ThirdAccountId, null),
            new MatchEntrant(FourthAccountId, null),
        };
        return Match.StartPrivate(new MatchFormat(gameMode, ClockMinutes), entrants, _startedAt);
    }

    private static Match StartMatch()
    {
        var gameMode = new GameMode
        {
            GameModeId = ClassicGameModeId,
            Code = GameModeCode.Classic,
            BoardSize = ClassicBoardSize,
            PlayerCount = ClassicPlayerCount,
            WallsPerPlayer = ClassicWallsPerPlayer,
            IsActive = true,
        };
        var entrants = new MatchEntrant[]
        {
            new MatchEntrant(AbandoningAccountId, StartingElo),
            new MatchEntrant(RivalAccountId, StartingElo),
        };
        return Match.StartRanked(new MatchFormat(gameMode, ClockMinutes), entrants, _startedAt);
    }
}
