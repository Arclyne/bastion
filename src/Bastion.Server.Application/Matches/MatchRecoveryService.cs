using System;
using System.Collections.Generic;
using Bastion.Domain.Matches;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class MatchRecoveryService : IMatchRecoveryService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchRecoveryService));

    private readonly IMatchRepository _matchRepository;

    public MatchRecoveryService(IMatchRepository matchRepository)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);

        _matchRepository = matchRepository;
    }

    /// <inheritdoc/>
    public void RecoverMatchesAfterRestart(DateTime restartedAt)
    {
        IReadOnlyList<Match> matches = _matchRepository.FindMatchesInProgress();
        foreach (Match match in matches)
        {
            match.RecoverAfterRestart(restartedAt);
        }

        _matchRepository.SaveRecoveredMatches(matches);
        _logger.Info($"The matches in progress were picked up after the restart. MatchCount={matches.Count}");
    }
}
