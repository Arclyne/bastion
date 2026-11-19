using System;
using Bastion.Domain.Common;
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
    private readonly IMatchCloser _matchCloser;

    public MatchAbandonmentService(
        ISessionVerifier sessionVerifier,
        IMatchRepository matchRepository,
        IMatchCloser matchCloser)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(matchCloser);

        _sessionVerifier = sessionVerifier;
        _matchRepository = matchRepository;
        _matchCloser = matchCloser;
    }

    /// <inheritdoc/>
    public void AbandonMatch(string sessionToken, int matchId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindAbandonableMatch(player, matchId);
        match.Abandon(player.AccountId, now);
        bool isMatchOver = match.IsFinished();
        SaveAbandonment(match);
        _logger.Info(
            $"A player left the match. AccountId={player.AccountId}; MatchId={matchId}; IsMatchOver={isMatchOver}");
        AnnounceAbandonment(match, player.AccountId, now);
    }

    /// <inheritdoc/>
    public AbandonmentPreview PreviewAbandonment(string sessionToken, int matchId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindAbandonableMatch(player, matchId);
        AbandonmentPreview preview = match.PreviewAbandonment(player.AccountId, now);
        return preview;
    }

    private Match FindAbandonableMatch(ConnectedPlayer player, int matchId)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound || !foundMatch.GetValue().CanBeAbandonedBy(player.AccountId))
        {
            _logger.Warn(
                "A player without an active participation tried to leave the match or see what leaving costs. "
                + $"AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        return foundMatch.GetValue();
    }

    private void SaveAbandonment(Match match)
    {
        if (match.IsFinished())
        {
            _matchCloser.CloseWithoutMove(match);
            return;
        }

        _matchRepository.SaveDeparture(match);
    }

    private void AnnounceAbandonment(Match match, int abandoningAccountId, DateTime now)
    {
        if (match.IsFinished())
        {
            _matchCloser.AnnounceEnd(match, abandoningAccountId);
            return;
        }

        _matchCloser.AnnounceDeparture(match, abandoningAccountId, now);
    }
}
