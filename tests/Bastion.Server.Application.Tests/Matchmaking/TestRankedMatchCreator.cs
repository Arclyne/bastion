using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Statistics;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matchmaking;

public class TestRankedMatchCreator
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
    private readonly RankedMatchCreator _rankedMatchCreator;
    private readonly MatchPairing _pairing;

    public TestRankedMatchCreator()
    {
        _rankedMatchCreator = new RankedMatchCreator(_matchRepository, _turnOrderShuffler);
        _matchRepository.NextMatchId = GeneratedMatchId;
        _matchRepository.EloRatingsByAccountId[FirstAccountId] = FirstEloRating;
        var players = new MatchSearch[]
        {
            new MatchSearch(FirstAccountId, FirstNickname, _startedAt),
            new MatchSearch(SecondAccountId, SecondNickname, _startedAt),
        };
        _pairing = new MatchPairing(players);
    }

    [Fact]
    public void TryCreateMatch_ActiveGameMode_SavesOneMatch()
    {
        _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out _);

        Assert.Single(_matchRepository.SavedMatches);
    }

    [Fact]
    public void TryCreateMatch_ActiveGameMode_ReturnsSavedMatchId()
    {
        _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out CreatedMatch? createdMatch);

        Assert.Equal(GeneratedMatchId, createdMatch?.MatchId);
    }

    [Fact]
    public void TryCreateMatch_ShuffleReversesPlayers_GivesFirstTurnToSecondPlayer()
    {
        _turnOrderShuffler.ShouldReverseOrder = true;

        _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out CreatedMatch? createdMatch);

        Assert.Equal(SecondNickname, FindParticipant(createdMatch, Match.FirstTurnOrder).Player.Nickname);
    }

    [Fact]
    public void TryCreateMatch_PlayerWithStatistic_KeepsTheirElo()
    {
        _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out CreatedMatch? createdMatch);

        Assert.Equal(FirstEloRating, FindParticipant(createdMatch, Match.FirstTurnOrder).EloRating);
    }

    [Fact]
    public void TryCreateMatch_PlayerWithoutStatistic_StartsAtDefaultElo()
    {
        _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out CreatedMatch? createdMatch);

        Assert.Equal(ModeStatistic.DefaultEloRating, FindParticipant(createdMatch, Match.SecondTurnOrder).EloRating);
    }

    [Fact]
    public void TryCreateMatch_InactiveGameMode_ReturnsFalse()
    {
        _matchRepository.ActiveGameMode = null;

        bool isCreated = _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out _);

        Assert.False(isCreated);
    }

    [Fact]
    public void TryCreateMatch_RepositoryFails_ReturnsFalse()
    {
        _matchRepository.MatchSaveFailure = new MatchRepositoryException(new InvalidOperationException(RepositoryFailureMessage));

        bool isCreated = _rankedMatchCreator.TryCreateMatch(_pairing, _startedAt, out _);

        Assert.False(isCreated);
    }

    private static CreatedMatchParticipant FindParticipant(CreatedMatch? createdMatch, byte turnOrder)
    {
        IReadOnlyList<CreatedMatchParticipant> participants =
            createdMatch?.Participants ?? Array.Empty<CreatedMatchParticipant>();
        return participants.First(participant => participant.TurnOrder == turnOrder);
    }
}
