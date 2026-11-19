using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;
using log4net;

namespace Bastion.Server.Application.Matches;

public sealed class MatchCloser : IMatchCloser
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchCloser));

    private readonly IMatchRepository _matchRepository;
    private readonly IMatchEndNotifier _matchEndNotifier;

    public MatchCloser(IMatchRepository matchRepository, IMatchEndNotifier matchEndNotifier)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(matchEndNotifier);

        _matchRepository = matchRepository;
        _matchEndNotifier = matchEndNotifier;
    }

    public void CloseAfterMove(Match finishedMatch)
    {
        MatchClosure closure = PrepareClosure(finishedMatch);
        _matchRepository.SaveFinalMove(closure);
        LogClosure(finishedMatch);
    }

    public void CloseWithoutMove(Match finishedMatch)
    {
        MatchClosure closure = PrepareClosure(finishedMatch);
        _matchRepository.SaveClosureWithoutMove(closure);
        LogClosure(finishedMatch);
    }

    public void AnnounceEnd(Match finishedMatch, int decidingAccountId)
    {
        ArgumentNullException.ThrowIfNull(finishedMatch);

        Participation deciding = finishedMatch.GetParticipation(decidingAccountId);
        MatchEndReason endReason = finishedMatch.EndReason.GetValueOrDefault();
        DateTime endedAt = finishedMatch.EndedAt.GetValueOrDefault();
        bool isRanked = finishedMatch.MatchType == MatchType.Ranked;
        int playedSeconds = CountPlayedSeconds(finishedMatch, endedAt);
        List<PlayerStanding> standings = ListStandings(finishedMatch);
        var notice = new MatchEndNotice
        {
            EndReason = endReason,
            DecidingTurnOrder = deciding.TurnOrder,
            IsRanked = isRanked,
            MoveCount = finishedMatch.Moves.Count,
            PlayedSeconds = playedSeconds,
            Standings = standings,
        };
        List<int> recipientAccountIds = ListRecipientAccountIds(finishedMatch, decidingAccountId);
        _matchEndNotifier.NotifyMatchEnded(finishedMatch.MatchId, recipientAccountIds, notice);
    }

    public void AnnounceDeparture(Match match, int departingAccountId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(match);

        Participation departing = match.GetParticipation(departingAccountId);
        MatchEndReason endReason = departing.EndReason.GetValueOrDefault();
        int activePlayerCount = match.CountActivePlayers();
        short nextMoveNumber = match.GetNextMoveNumber();
        int playedSeconds = CountPlayedSeconds(match, now);
        List<PlayerStanding> standings = ListStandings(match);
        var notice = new ParticipationEndNotice
        {
            EndReason = endReason,
            DepartingTurnOrder = departing.TurnOrder,
            ActivePlayerCount = activePlayerCount,
            NextTurnOrder = match.CurrentTurnOrder,
            NextMoveNumber = nextMoveNumber,
            MoveCount = match.Moves.Count,
            PlayedSeconds = playedSeconds,
            Standings = standings,
        };
        List<int> recipientAccountIds = ListRecipientAccountIds(match, departingAccountId);
        _matchEndNotifier.NotifyParticipationEnded(match.MatchId, recipientAccountIds, notice);
    }

    public void ExpireClock(Match match, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(match);

        Participation timedOut = match.GetTurnParticipation();
        match.ExpireClock(now);
        _logger.Info($"A clock ran out. AccountId={timedOut.AccountId}; MatchId={match.MatchId}");
        if (match.IsFinished())
        {
            CloseWithoutMove(match);
            AnnounceEnd(match, timedOut.AccountId);
            return;
        }

        _matchRepository.SaveDeparture(match);
        AnnounceDeparture(match, timedOut.AccountId, now);
    }

    private MatchClosure PrepareClosure(Match finishedMatch)
    {
        ArgumentNullException.ThrowIfNull(finishedMatch);

        if (!finishedMatch.IsFinished())
        {
            throw new InvalidOperationException(
                $"Only a finished match can be closed. MatchId={finishedMatch.MatchId}");
        }

        if (finishedMatch.MatchType != MatchType.Ranked)
        {
            return new MatchClosure(finishedMatch, [], []);
        }

        if (!finishedMatch.CanBeRated())
        {
            _logger.Error(
                "A ranked match closed without elo because a player has no initial elo. "
                + $"MatchId={finishedMatch.MatchId}");
            return new MatchClosure(finishedMatch, [], []);
        }

        finishedMatch.Rate();
        return RecordStatistics(finishedMatch);
    }

    private MatchClosure RecordStatistics(Match ratedMatch)
    {
        List<int> accountIds = ListParticipantAccountIds(ratedMatch);
        IReadOnlyList<ModeStatistic> storedStatistics = _matchRepository.FindModeStatistics(
            ratedMatch.GameModeId,
            accountIds);
        List<ModeStatistic> startedStatistics = [];
        foreach (Participation participation in ratedMatch.Participations)
        {
            ModeStatistic? statistic = storedStatistics.FirstOrDefault(
                candidate => candidate.AccountId == participation.AccountId);
            if (statistic is null)
            {
                statistic = ModeStatistic.StartFor(participation.AccountId, ratedMatch.GameModeId);
                startedStatistics.Add(statistic);
            }

            RecordParticipation(ratedMatch, participation, statistic);
        }

        return new MatchClosure(ratedMatch, storedStatistics, startedStatistics);
    }

    private static List<int> ListParticipantAccountIds(Match match)
    {
        IEnumerable<int> participantAccountIds = match.Participations.Select(participation => participation.AccountId);
        return [.. participantAccountIds];
    }

    private static void RecordParticipation(Match ratedMatch, Participation participation, ModeStatistic statistic)
    {
        ParticipationResult result = participation.Result.GetValueOrDefault();
        short finalElo = participation.FinalElo.GetValueOrDefault();
        DateTime endedAt = ratedMatch.EndedAt.GetValueOrDefault();
        statistic.RecordMatch(result, finalElo, endedAt);
        if (ratedMatch.HasAbandoned(participation.AccountId))
        {
            statistic.RecordAbandonment();
        }
    }

    private static void LogClosure(Match closedMatch)
    {
        _logger.Info(
            $"A match closed. MatchId={closedMatch.MatchId}; EndReason={closedMatch.EndReason}; "
            + $"MatchType={closedMatch.MatchType}");
    }

    private static int CountPlayedSeconds(Match match, DateTime until)
    {
        TimeSpan playedTime = until - match.StartedAt;
        double playedSeconds = Math.Max(playedTime.TotalSeconds, 0);
        int wholeSeconds = (int)playedSeconds;
        return wholeSeconds;
    }

    private static List<PlayerStanding> ListStandings(Match match)
    {
        IEnumerable<PlayerStanding> standings = match.Participations
            .OrderBy(participation => participation.TurnOrder)
            .Select(ToStanding);
        return [.. standings];
    }

    private static PlayerStanding ToStanding(Participation participation)
    {
        return new PlayerStanding
        {
            TurnOrder = participation.TurnOrder,
            Result = participation.Result,
            Placement = participation.Placement,
            EndReason = participation.EndReason,
            InitialElo = participation.InitialElo,
            FinalElo = participation.FinalElo,
            RemainingWalls = participation.RemainingWalls,
        };
    }

    private static List<int> ListRecipientAccountIds(Match match, int decidingAccountId)
    {
        IEnumerable<int> recipientAccountIds = match.Participations
            .Select(participation => participation.AccountId)
            .Where(accountId => accountId == decidingAccountId || !match.HasAbandoned(accountId));
        return [.. recipientAccountIds];
    }
}
