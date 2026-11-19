using System;
using System.Collections.Generic;
using Bastion.Domain.Common;
using Bastion.Domain.Matches;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class MatchClockService : IMatchClockService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchClockService));

    private readonly IMatchRepository _matchRepository;
    private readonly IMatchCloser _matchCloser;
    private readonly IMatchGate _matchGate;

    public MatchClockService(IMatchRepository matchRepository, IMatchCloser matchCloser, IMatchGate matchGate)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(matchCloser);
        ArgumentNullException.ThrowIfNull(matchGate);

        _matchRepository = matchRepository;
        _matchCloser = matchCloser;
        _matchGate = matchGate;
    }

    /// <inheritdoc/>
    public void ExpireRunOutClocks(DateTime now)
    {
        IReadOnlyList<int> matchIds = _matchRepository.FindMatchIdsWithRunOutClock(now);
        foreach (int matchId in matchIds)
        {
            Action expireClock = () => ExpireClockOfMatch(matchId, now);
            TryExpireClock(matchId, expireClock);
        }
    }

    /// <inheritdoc/>
    public void TakeOutAbsentPlayers(DateTime now)
    {
        IReadOnlyList<int> matchIds = _matchRepository.FindMatchIdsWithAbsentPlayer(now);
        foreach (int matchId in matchIds)
        {
            Action takeOutAbsentPlayer = () => TakeOutAbsentPlayerOfMatch(matchId, now);
            TryExpireClock(matchId, takeOutAbsentPlayer);
        }
    }

    private void TryExpireClock(int matchId, Action expireClock)
    {
        try
        {
            _matchGate.Run(matchId, expireClock);
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"A timeout of the match could not be applied; it is tried again. MatchId={matchId}", ex);
        }
    }

    private void ExpireClockOfMatch(int matchId, DateTime now)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound)
        {
            return;
        }

        Match match = foundMatch.GetValue();
        if (!match.HasClockRunOut(now))
        {
            return;
        }

        _matchCloser.ExpireClock(match, now);
    }

    private void TakeOutAbsentPlayerOfMatch(int matchId, DateTime now)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound)
        {
            return;
        }

        Match match = foundMatch.GetValue();
        if (!match.HasAbsentPlayerToTakeOut(now))
        {
            return;
        }

        _matchCloser.TakeOutAbsentPlayer(match, now);
    }
}
