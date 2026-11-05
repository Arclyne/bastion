using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    private readonly IMatchStore _matchStore;
    private readonly ITurnOrderShuffler _turnOrderShuffler;

    public RankedMatchCreator(IMatchStore matchStore, ITurnOrderShuffler turnOrderShuffler)
    {
        ArgumentNullException.ThrowIfNull(matchStore);
        ArgumentNullException.ThrowIfNull(turnOrderShuffler);

        _matchStore = matchStore;
        _turnOrderShuffler = turnOrderShuffler;
    }

    public bool TryCreateMatch(
        MatchPairing pairing,
        DateTime startedAt,
        [NotNullWhen(true)]
        out CreatedMatch? createdMatch)
    {
        ArgumentNullException.ThrowIfNull(pairing);

        createdMatch = null;
        string accountIdsText = string.Join(AccountIdSeparator, pairing.Players.Select(player => player.AccountId));
        try
        {
            GameMode? gameMode = _matchStore.FindActiveGameMode(MatchmakingRules.RankedGameModeCode);
            if (gameMode is null)
            {
                _logger.Error(
                    $"The ranked match was not created because its game mode is inactive. AccountIds={accountIdsText}");
                return false;
            }

            var format = new MatchFormat(gameMode, MatchmakingRules.RankedClockMinutes);
            createdMatch = StartAndSaveMatch(pairing, format, startedAt);
            _logger.Info($"Ranked match created. MatchId={createdMatch.MatchId}; AccountIds={accountIdsText}");
            return true;
        }
        catch (MatchStoreException ex)
        {
            _logger.Error($"The ranked match could not be saved. AccountIds={accountIdsText}", ex);
            return false;
        }
    }

    private static List<CreatedMatchParticipant> ToParticipants(Match match, IReadOnlyList<MatchSearch> players)
    {
        var participants = new List<CreatedMatchParticipant>();
        foreach (Participation participation in match.Participations)
        {
            MatchSearch search = players.First(player => player.AccountId == participation.AccountId);
            var connectedPlayer = new ConnectedPlayer(search.AccountId, search.Nickname);
            participants.Add(
                new CreatedMatchParticipant(connectedPlayer, participation.InitialElo, participation.TurnOrder));
        }

        return participants;
    }

    private CreatedMatch StartAndSaveMatch(MatchPairing pairing, MatchFormat format, DateTime startedAt)
    {
        IReadOnlyList<MatchSearch> playersInTurnOrder = _turnOrderShuffler.ShuffleTurnOrder(pairing.Players);
        List<MatchEntrant> entrants = CreateEntrants(playersInTurnOrder, format.GameMode.GameModeId);
        var match = Match.StartRanked(format, entrants, startedAt);
        int matchId = _matchStore.SaveMatch(match);
        return new CreatedMatch(matchId, format, ToParticipants(match, playersInTurnOrder));
    }

    private List<MatchEntrant> CreateEntrants(IReadOnlyList<MatchSearch> playersInTurnOrder, byte gameModeId)
    {
        int[] accountIds = playersInTurnOrder.Select(player => player.AccountId).ToArray();
        IReadOnlyDictionary<int, short> eloRatingsByAccountId =
            _matchStore.FindEloRatingsByAccountId(gameModeId, accountIds);

        var entrants = new List<MatchEntrant>();
        foreach (int accountId in accountIds)
        {
            // Registration gives each account a row per active mode (CU-02 RN-06); a mode activated later has none.
            short eloRating = eloRatingsByAccountId.GetValueOrDefault(accountId, ModeStatistic.DefaultEloRating);
            entrants.Add(new MatchEntrant(accountId, eloRating));
        }

        return entrants;
    }
}
