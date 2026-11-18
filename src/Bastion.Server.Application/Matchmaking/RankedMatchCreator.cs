using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Statistics;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Matchmaking;

public sealed class RankedMatchCreator : IRankedMatchCreator
{
    private const string AccountIdSeparator = ",";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RankedMatchCreator));

    private readonly IMatchRepository _matchRepository;
    private readonly ITurnOrderShuffler _turnOrderShuffler;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public RankedMatchCreator(
        IMatchRepository matchRepository,
        ITurnOrderShuffler turnOrderShuffler,
        IMatchmakingNotifier matchmakingNotifier)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(turnOrderShuffler);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);

        _matchRepository = matchRepository;
        _turnOrderShuffler = turnOrderShuffler;
        _matchmakingNotifier = matchmakingNotifier;
    }

    public void CreateMatch(MatchPairing pairing, DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(pairing);

        IEnumerable<int> pairedAccountIds = pairing.Players.Select(player => player.AccountId);
        string accountIdsText = string.Join(AccountIdSeparator, pairedAccountIds);
        try
        {
            Lookup<GameMode> foundGameMode = _matchRepository.FindActiveGameMode(MatchmakingPolicy.RankedGameModeCode);
            if (!foundGameMode.IsFound)
            {
                _logger.Error(
                    "The ranked match was not created because no active game mode has its code. "
                    + $"AccountIds={accountIdsText}");
                NotifyMatchCreationFailed(pairing);
                return;
            }

            GameMode gameMode = foundGameMode.GetValue();
            if (gameMode.PlayerCount != pairing.Players.Count)
            {
                _logger.Error(
                    "The ranked match was not created because its game mode seats another number of players. "
                    + $"PlayerCount={gameMode.PlayerCount}; AccountIds={accountIdsText}");
                NotifyMatchCreationFailed(pairing);
                return;
            }

            var format = new MatchFormat(gameMode, MatchmakingPolicy.RankedClockMinutes);
            CreatedMatch createdMatch = StartAndSaveMatch(pairing, format, startedAt);
            _logger.Info($"Ranked match created. MatchId={createdMatch.MatchId}; AccountIds={accountIdsText}");
            _matchmakingNotifier.NotifyMatchFound(createdMatch);
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"The ranked match could not be created. AccountIds={accountIdsText}", ex);
            NotifyMatchCreationFailed(pairing);
        }
    }

    private void NotifyMatchCreationFailed(MatchPairing pairing)
    {
        foreach (MatchSearch matchSearch in pairing.Players)
        {
            _matchmakingNotifier.NotifySearchEnded(matchSearch.AccountId, SearchEndReason.MatchCreationFailed);
        }
    }

    private CreatedMatch StartAndSaveMatch(MatchPairing pairing, MatchFormat format, DateTime startedAt)
    {
        IReadOnlyList<MatchSearch> playersInTurnOrder = _turnOrderShuffler.ShuffleTurnOrder(pairing.Players);
        List<MatchEntrant> entrants = CreateEntrants(playersInTurnOrder, format.GameMode.GameModeId);
        var match = Match.StartRanked(format, entrants, startedAt);
        int matchId = _matchRepository.SaveMatch(match);
        List<CreatedMatchParticipant> participants = ToParticipants(match, playersInTurnOrder);
        return new CreatedMatch(matchId, format, participants);
    }

    private List<MatchEntrant> CreateEntrants(IReadOnlyList<MatchSearch> playersInTurnOrder, byte gameModeId)
    {
        IEnumerable<int> accountIdsInTurnOrder = playersInTurnOrder.Select(player => player.AccountId);
        var accountIds = new List<int>(accountIdsInTurnOrder);
        IReadOnlyDictionary<int, short> eloRatingsByAccountId = _matchRepository.FindEloRatingsByAccountId(
            gameModeId,
            accountIds);

        var entrants = new List<MatchEntrant>();
        foreach (int accountId in accountIds)
        {
            short eloRating = eloRatingsByAccountId.GetValueOrDefault(accountId, ModeStatistic.DefaultEloRating);
            var entrant = new MatchEntrant(accountId, eloRating);
            entrants.Add(entrant);
        }

        return entrants;
    }

    private static List<CreatedMatchParticipant> ToParticipants(Match match, IReadOnlyList<MatchSearch> players)
    {
        var participants = new List<CreatedMatchParticipant>();
        foreach (Participation participation in match.Participations)
        {
            MatchSearch matchSearch = players.First(player => player.AccountId == participation.AccountId);
            var connectedPlayer = new ConnectedPlayer(matchSearch.AccountId, matchSearch.Nickname);
            var participant = new CreatedMatchParticipant(
                connectedPlayer,
                participation.InitialElo,
                participation.TurnOrder);
            participants.Add(participant);
        }

        return participants;
    }
}
