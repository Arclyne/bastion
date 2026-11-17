using System;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Statistics;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public sealed class TestRankedMatchCreator
{
    private const int FirstAccountId = 1;
    private const int SecondAccountId = 2;
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "leo_peon";
    private const short FirstEloRating = 1210;
    private const int GeneratedMatchId = 42;
    private const string RepositoryFailureMessage = "The database is not reachable.";

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeTurnOrderShuffler _turnOrderShuffler = new FakeTurnOrderShuffler();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly RankedMatchCreator _rankedMatchCreator;
    private readonly MatchPairing _pairing;

    public TestRankedMatchCreator()
    {
        _rankedMatchCreator = new RankedMatchCreator(_matchRepository, _turnOrderShuffler, _matchmakingNotifier);
        _pairing = CreatePairing(FirstNickname);
    }

    [Fact]
    public void CreateMatch_ActiveGameMode_SavesOneMatch()
    {
        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        Assert.Single(_matchRepository.SavedMatches);
    }

    [Fact]
    public void CreateMatch_ActiveGameMode_NotifiesSavedMatchId()
    {
        _matchRepository.NextMatchId = GeneratedMatchId;

        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        CreatedMatch createdMatch = _matchmakingNotifier.CreatedMatches.Single();
        Assert.Equal(GeneratedMatchId, createdMatch.MatchId);
    }

    [Fact]
    public void CreateMatch_ShuffleReversesPlayers_GivesFirstTurnToSecondPlayer()
    {
        _turnOrderShuffler.ShouldReverseOrder = true;

        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        CreatedMatchParticipant firstPlayer = FindParticipant(Match.FirstTurnOrder);
        Assert.Equal(SecondNickname, firstPlayer.Player.Nickname);
    }

    [Theory]
    [InlineData("rival_one")]
    [InlineData("peón_ñandú")]
    public void CreateMatch_EnglishOrSpanishNickname_KeepsSameNickname(string nickname)
    {
        MatchPairing pairing = CreatePairing(nickname);

        _rankedMatchCreator.CreateMatch(pairing, _startedAt);

        CreatedMatchParticipant firstPlayer = FindParticipant(Match.FirstTurnOrder);
        Assert.Equal(nickname, firstPlayer.Player.Nickname);
    }

    [Fact]
    public void CreateMatch_PlayerWithStatistic_KeepsTheirElo()
    {
        _matchRepository.EloRatingsByAccountId[FirstAccountId] = FirstEloRating;

        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        CreatedMatchParticipant firstPlayer = FindParticipant(Match.FirstTurnOrder);
        Assert.Equal(FirstEloRating, firstPlayer.EloRating);
    }

    [Fact]
    public void CreateMatch_PlayerWithoutStatistic_StartsAtDefaultElo()
    {
        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        CreatedMatchParticipant secondPlayer = FindParticipant(Match.SecondTurnOrder);
        Assert.Equal(ModeStatistic.DefaultEloRating, secondPlayer.EloRating);
    }

    [Fact]
    public void CreateMatch_InactiveGameMode_NotifiesMatchCreationFailed()
    {
        _matchRepository.ActiveGameMode = null;

        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        SearchEndReason reason = _matchmakingNotifier.EndReasonsByAccountId[FirstAccountId];
        Assert.Equal(SearchEndReason.MatchCreationFailed, reason);
    }

    [Fact]
    public void CreateMatch_FourSeatGameMode_NotifiesMatchCreationFailed()
    {
        _matchRepository.ActiveGameMode = GameModeSampleFactory.CreateClassicWithFourSeats();

        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        SearchEndReason reason = _matchmakingNotifier.EndReasonsByAccountId[FirstAccountId];
        Assert.Equal(SearchEndReason.MatchCreationFailed, reason);
    }

    [Fact]
    public void CreateMatch_RepositoryFails_NotifiesMatchCreationFailed()
    {
        var cause = new InvalidOperationException(RepositoryFailureMessage);
        _matchRepository.MatchSaveFailure = new MatchRepositoryException(cause);

        _rankedMatchCreator.CreateMatch(_pairing, _startedAt);

        SearchEndReason reason = _matchmakingNotifier.EndReasonsByAccountId[FirstAccountId];
        Assert.Equal(SearchEndReason.MatchCreationFailed, reason);
    }

    private static MatchPairing CreatePairing(string firstNickname)
    {
        var firstSearch = new MatchSearch(FirstAccountId, firstNickname, _startedAt);
        var secondSearch = new MatchSearch(SecondAccountId, SecondNickname, _startedAt);
        MatchSearch[] players = { firstSearch, secondSearch };
        return new MatchPairing(players);
    }

    private CreatedMatchParticipant FindParticipant(byte turnOrder)
    {
        CreatedMatch createdMatch = _matchmakingNotifier.CreatedMatches.Single();
        return createdMatch.Participants.First(participant => participant.TurnOrder == turnOrder);
    }
}
