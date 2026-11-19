using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Common;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class MatchDrawService : IMatchDrawService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchDrawService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchCloser _matchCloser;
    private readonly IMatchDrawNotifier _drawNotifier;

    public MatchDrawService(ISessionVerifier sessionVerifier, IMatchRepository matchRepository, MatchDrawFollowUp followUp)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(followUp);

        _sessionVerifier = sessionVerifier;
        _matchRepository = matchRepository;
        _matchCloser = followUp.Closer;
        _drawNotifier = followUp.DrawNotifier;
    }

    /// <inheritdoc/>
    public void OfferDraw(string sessionToken, int matchId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindPlayedMatch(player, matchId, now);
        DrawOfferRejection rejection = match.FindDrawOfferRejection(player.AccountId);
        if (rejection.Reason != DrawOfferRejectionReason.None)
        {
            _logger.Info(
                $"A draw offer was turned down. AccountId={player.AccountId}; MatchId={matchId}; "
                + $"Reason={rejection.Reason}; MovesLeft={rejection.MovesLeft}");
            throw new DrawOfferRejectedException(rejection);
        }

        DrawOfferOutcome outcome = match.OfferDraw(player.AccountId, now);
        if (outcome == DrawOfferOutcome.Agreed)
        {
            CloseByAgreement(match, player);
            return;
        }

        _matchRepository.SaveDrawOffer(match);
        _logger.Info($"A player offered a draw. AccountId={player.AccountId}; MatchId={matchId}");
        Participation offerer = match.GetParticipation(player.AccountId);
        List<int> recipientAccountIds = ListOtherAccountIds(match, player.AccountId);
        _drawNotifier.NotifyDrawOffered(matchId, recipientAccountIds, offerer.TurnOrder);
    }

    /// <inheritdoc/>
    public void RespondToDraw(string sessionToken, DrawResponse response, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(response);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindPlayedMatch(player, response.MatchId, now);
        if (!match.HasPendingDrawOfferFor(player.AccountId))
        {
            throw new DrawNotPendingException();
        }

        if (response.IsAccepted)
        {
            match.AcceptDraw(player.AccountId, now);
            CloseByAgreement(match, player);
            return;
        }

        match.RejectDraw(player.AccountId, now);
        _matchRepository.SaveDrawOffer(match);
        _logger.Info($"A player rejected a draw. AccountId={player.AccountId}; MatchId={response.MatchId}");
        List<int> recipientAccountIds = ListOtherAccountIds(match, player.AccountId);
        _drawNotifier.NotifyDrawRejected(response.MatchId, recipientAccountIds);
    }

    private Match FindPlayedMatch(ConnectedPlayer player, int matchId, DateTime now)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound || !foundMatch.GetValue().IsPlayedBy(player.AccountId))
        {
            _logger.Warn(
                "A player without an active participation tried to offer or answer a draw. "
                + $"AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        Match match = foundMatch.GetValue();
        if (match.HasClockRunOut(now))
        {
            _matchCloser.ExpireClock(match, now);
        }

        if (!match.IsPlayedBy(player.AccountId))
        {
            throw new MatchNotJoinedException();
        }

        return match;
    }

    private void CloseByAgreement(Match match, ConnectedPlayer player)
    {
        _matchCloser.CloseWithoutMove(match);
        _logger.Info($"The players agreed a draw. AccountId={player.AccountId}; MatchId={match.MatchId}");
        _matchCloser.AnnounceEnd(match, player.AccountId);
    }

    private static List<int> ListOtherAccountIds(Match match, int accountId)
    {
        IEnumerable<int> otherAccountIds = match.Participations
            .Where(participation => participation.AccountId != accountId)
            .Select(participation => participation.AccountId);
        return [.. otherAccountIds];
    }
}
