using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class MatchAbandonmentService : IMatchAbandonmentService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchAbandonmentService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchStore _matchStore;
    private readonly IMatchEndNotifier _matchEndNotifier;

    public MatchAbandonmentService(
        ISessionVerifier sessionVerifier,
        IMatchStore matchStore,
        IMatchEndNotifier matchEndNotifier)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchStore);
        ArgumentNullException.ThrowIfNull(matchEndNotifier);

        _sessionVerifier = sessionVerifier;
        _matchStore = matchStore;
        _matchEndNotifier = matchEndNotifier;
    }

    /// <inheritdoc/>
    public void AbandonMatch(string sessionToken, int matchId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match? match = _matchStore.FindMatch(matchId);
        if (match is null || !match.CanBeAbandonedBy(player.AccountId))
        {
            _logger.Warn(
                $"A player tried to leave a match they do not play. AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        match.EndByAbandonment(player.AccountId, now);
        _matchStore.SaveMatchEnd(match);
        _logger.Info(
            $"A player left; the match ends as an abandonment. AccountId={player.AccountId}; MatchId={matchId}");
        IEnumerable<int> rivalAccountIds = match.Participations
            .Where(participation => participation.AccountId != player.AccountId)
            .Select(participation => participation.AccountId);
        _matchEndNotifier.NotifyMatchAbandoned(matchId, new List<int>(rivalAccountIds), player.Nickname);
    }
}
