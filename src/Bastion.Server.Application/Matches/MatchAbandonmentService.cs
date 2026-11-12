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
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchEndNotifier _matchEndNotifier;

    public MatchAbandonmentService(
        ISessionVerifier sessionVerifier,
        IMatchRepository matchRepository,
        IMatchEndNotifier matchEndNotifier)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(matchEndNotifier);

        _sessionVerifier = sessionVerifier;
        _matchRepository = matchRepository;
        _matchEndNotifier = matchEndNotifier;
    }

    /// <inheritdoc/>
    public void AbandonMatch(string sessionToken, int matchId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match? match = _matchRepository.FindMatch(matchId);
        if (match is null || !match.CanBeAbandonedBy(player.AccountId))
        {
            _logger.Warn(
                $"A player tried to leave a match they do not play. AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        match.Abandon(player.AccountId, now);
        bool isMatchOver = match.Status == MatchStatus.Finished;
        _matchRepository.SaveAbandonment(match);
        _logger.Info(
            $"A player left the match. AccountId={player.AccountId}; MatchId={matchId}; IsMatchOver={isMatchOver}");
        IEnumerable<int> recipientAccountIds = match.Participations
            .Where(participation => participation.AccountId != player.AccountId && !participation.EndReason.HasValue)
            .Select(participation => participation.AccountId);
        var recipientAccountIdList = new List<int>(recipientAccountIds);
        var notice = new AbandonmentNotice(player.Nickname, isMatchOver);
        _matchEndNotifier.NotifyMatchAbandoned(matchId, recipientAccountIdList, notice);
    }
}
