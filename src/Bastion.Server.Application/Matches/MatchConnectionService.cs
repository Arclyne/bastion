using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class MatchConnectionService : IMatchConnectionService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchConnectionService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchCloser _matchCloser;
    private readonly IMatchConnectionNotifier _connectionNotifier;
    private readonly IMatchGate _matchGate;

    public MatchConnectionService(
        ISessionVerifier sessionVerifier,
        IMatchRepository matchRepository,
        MatchConnectionFollowUp followUp)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(followUp);

        _sessionVerifier = sessionVerifier;
        _matchRepository = matchRepository;
        _matchCloser = followUp.Closer;
        _connectionNotifier = followUp.ConnectionNotifier;
        _matchGate = followUp.Gate;
    }

    /// <inheritdoc/>
    public void MarkDisconnected(int accountId, DateTime now)
    {
        Lookup<int> foundMatchId = _matchRepository.FindUnfinishedMatchId(accountId);
        if (!foundMatchId.IsFound)
        {
            return;
        }

        int matchId = foundMatchId.GetValue();
        Action disconnectFromMatch = () => DisconnectFromMatch(accountId, matchId, now);
        _matchGate.Run(matchId, disconnectFromMatch);
    }

    /// <inheritdoc/>
    public Lookup<MatchSnapshot> FindOngoingMatch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Lookup<int> foundMatchId = _matchRepository.FindUnfinishedMatchId(player.AccountId);
        if (!foundMatchId.IsFound)
        {
            return Lookup.Missing<MatchSnapshot>($"AccountId={player.AccountId}");
        }

        Lookup<Match> foundMatch = _matchRepository.FindMatch(foundMatchId.GetValue());
        if (!foundMatch.IsFound)
        {
            return Lookup.Missing<MatchSnapshot>($"AccountId={player.AccountId}");
        }

        MatchSnapshot snapshot = TakeSnapshot(foundMatch.GetValue(), player.AccountId, now);
        return Lookup.Found(snapshot);
    }

    /// <inheritdoc/>
    public MatchSnapshot RejoinMatch(string sessionToken, int matchId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindParticipatedMatch(player, matchId);
        if (match.HasClockRunOut(now))
        {
            _matchCloser.ExpireClock(match, now);
        }

        if (match.IsFinished())
        {
            int decidingAccountId = FindDecidingAccountId(match);
            MatchEndNotice ending = _matchCloser.DescribeEnd(match, decidingAccountId);
            MatchSnapshot endedSnapshot = TakeSnapshot(match, player.AccountId, now) with { Ending = ending };
            return endedSnapshot;
        }

        if (!match.IsPlayedBy(player.AccountId))
        {
            ParticipationEndNotice ownDeparture = _matchCloser.DescribeDeparture(match, player.AccountId, now);
            MatchSnapshot departedSnapshot = TakeSnapshot(match, player.AccountId, now) with { OwnDeparture = ownDeparture };
            return departedSnapshot;
        }

        ReconnectToMatch(match, player.AccountId);
        return TakeSnapshot(match, player.AccountId, now);
    }

    private void DisconnectFromMatch(int accountId, int matchId, DateTime now)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound || !foundMatch.GetValue().IsPlayedBy(accountId))
        {
            return;
        }

        Match match = foundMatch.GetValue();
        Participation disconnecting = match.GetParticipation(accountId);
        if (!disconnecting.IsConnected)
        {
            return;
        }

        match.Disconnect(accountId, now);
        _matchRepository.SaveConnection(match);
        _logger.Info($"A player lost the connection to the match. AccountId={accountId}; MatchId={matchId}");
        var disconnection = new OpponentDisconnection(disconnecting.TurnOrder, now);
        List<int> recipientAccountIds = ListOtherAccountIds(match, accountId);
        _connectionNotifier.NotifyOpponentDisconnected(matchId, recipientAccountIds, disconnection);
    }

    private static List<int> ListOtherAccountIds(Match match, int accountId)
    {
        IEnumerable<int> otherAccountIds = match.Participations
            .Where(participation => participation.AccountId != accountId)
            .Select(participation => participation.AccountId);
        return [.. otherAccountIds];
    }

    private MatchSnapshot TakeSnapshot(Match match, int requestingAccountId, DateTime now)
    {
        byte boardSize = FindBoardSize(match);
        IEnumerable<int> participantAccountIds = match.Participations.Select(participation => participation.AccountId);
        List<int> accountIds = [.. participantAccountIds];
        IReadOnlyDictionary<int, string> nicknames = _matchRepository.FindNicknames(accountIds);
        return new MatchSnapshot
        {
            Match = match,
            BoardSize = boardSize,
            NicknamesByAccountId = nicknames,
            TakenAt = now,
            RequestingAccountId = requestingAccountId,
        };
    }

    private byte FindBoardSize(Match match)
    {
        Lookup<GameMode> foundGameMode = _matchRepository.FindGameMode(match.GameModeId);
        if (!foundGameMode.IsFound)
        {
            throw new InvalidOperationException($"The game mode of the match does not exist. MatchId={match.MatchId}");
        }

        GameMode gameMode = foundGameMode.GetValue();
        return gameMode.BoardSize;
    }

    private Match FindParticipatedMatch(ConnectedPlayer player, int matchId)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound || !HasParticipation(foundMatch.GetValue(), player.AccountId))
        {
            _logger.Warn(
                "A player tried to return to or claim in a match they never played. "
                + $"AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        return foundMatch.GetValue();
    }

    private static bool HasParticipation(Match match, int accountId)
    {
        return match.Participations.Any(participation => participation.AccountId == accountId);
    }

    private static int FindDecidingAccountId(Match finishedMatch)
    {
        if (finishedMatch.EndReason == MatchEndReason.Goal)
        {
            Participation winner = finishedMatch.Participations.First(
                participation => participation.Result == ParticipationResult.Won);
            return winner.AccountId;
        }

        Participation? decidingLoser = finishedMatch.Participations.FirstOrDefault(
            participation => participation.EndReason == finishedMatch.EndReason);
        Participation loser = decidingLoser ?? finishedMatch.Participations.First(
            participation => participation.Result == ParticipationResult.Lost);
        return loser.AccountId;
    }

    private void ReconnectToMatch(Match match, int accountId)
    {
        Participation returning = match.GetParticipation(accountId);
        if (returning.IsConnected)
        {
            return;
        }

        match.Reconnect(accountId);
        _matchRepository.SaveConnection(match);
        _logger.Info($"A player came back to the match. AccountId={accountId}; MatchId={match.MatchId}");
        List<int> recipientAccountIds = ListOtherAccountIds(match, accountId);
        _connectionNotifier.NotifyOpponentReconnected(match.MatchId, recipientAccountIds, returning.TurnOrder);
    }

}
