using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class PawnMoveService : IPawnMoveService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PawnMoveService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchMoveNotifier _matchMoveNotifier;

    public PawnMoveService(
        ISessionVerifier sessionVerifier,
        IMatchRepository matchRepository,
        IMatchMoveNotifier matchMoveNotifier)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(matchMoveNotifier);

        _sessionVerifier = sessionVerifier;
        _matchRepository = matchRepository;
        _matchMoveNotifier = matchMoveNotifier;
    }

    /// <inheritdoc/>
    public void MovePawn(string sessionToken, ChosenPawnMove move, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(move);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindPlayedMatch(player, move.MatchId);
        EnsureTurn(match, player, move);
        BoardCoordinates destination = ReadDestination(player, move);
        var pawnMove = new PawnMove(player.AccountId, move.MoveNumber, destination);
        byte boardSize = FindBoardSize(match);
        EnsureReachable(match, pawnMove, boardSize);
        Participation mover = match.Participations.Single(participation => participation.AccountId == player.AccountId);
        string fromSquare = mover.CurrentSquare;
        match.MovePawn(pawnMove, boardSize, now);
        _matchRepository.SavePawnMove(match);
        _logger.Info(
            $"A pawn moved. AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={move.MoveNumber}");
        NotifyPawnMoved(match, player, fromSquare);
    }

    private static void EnsureTurn(Match match, ConnectedPlayer player, ChosenPawnMove move)
    {
        short nextMoveNumber = match.GetNextMoveNumber();
        if (match.IsTurnOf(player.AccountId) && move.MoveNumber == nextMoveNumber)
        {
            return;
        }

        if (move.MoveNumber > nextMoveNumber)
        {
            _logger.Warn(
                "A move arrived ahead of the expected number; the client may be modified. "
                + $"AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={move.MoveNumber}; "
                + $"NextMoveNumber={nextMoveNumber}");
        }

        throw new NotYourTurnException(match.CurrentTurnOrder, nextMoveNumber);
    }

    private static BoardCoordinates ReadDestination(ConnectedPlayer player, ChosenPawnMove move)
    {
        if (!BoardLayout.TryParseSquare(move.DestinationSquare, out BoardCoordinates destination))
        {
            _logger.Warn(
                $"A move arrived with a square outside board notation. AccountId={player.AccountId}; "
                + $"MatchId={move.MatchId}; Square={move.DestinationSquare}");
            throw new IllegalMoveException();
        }

        return destination;
    }

    private static void EnsureReachable(Match match, PawnMove pawnMove, byte boardSize)
    {
        IReadOnlyList<BoardCoordinates> reachableSquares = match.ListReachableSquares(pawnMove.AccountId, boardSize);
        if (reachableSquares.Contains(pawnMove.Destination))
        {
            return;
        }

        string destinationSquare = BoardLayout.ToSquare(pawnMove.Destination);
        _logger.Warn(
            $"The server rejected a pawn move. AccountId={pawnMove.AccountId}; MatchId={match.MatchId}; "
            + $"MoveNumber={pawnMove.MoveNumber}; Square={destinationSquare}");
        throw new IllegalMoveException();
    }

    private Match FindPlayedMatch(ConnectedPlayer player, int matchId)
    {
        Match? match = _matchRepository.FindMatch(matchId);
        if (match is null || !match.IsPlayedBy(player.AccountId))
        {
            _logger.Warn(
                $"A player tried to move in a match they do not play. AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        return match;
    }

    private byte FindBoardSize(Match match)
    {
        GameMode? gameMode = _matchRepository.FindGameMode(match.GameModeId);
        if (gameMode is null)
        {
            throw new InvalidOperationException($"The game mode of the match does not exist. MatchId={match.MatchId}");
        }

        return gameMode.BoardSize;
    }

    private void NotifyPawnMoved(Match match, ConnectedPlayer player, string fromSquare)
    {
        Participation mover = match.Participations.Single(participation => participation.AccountId == player.AccountId);
        short nextMoveNumber = match.GetNextMoveNumber();
        bool isMatchOver = match.Status == MatchStatus.Finished;
        var notice = new PawnMoveNotice
        {
            MoverNickname = player.Nickname,
            FromSquare = fromSquare,
            ToSquare = mover.CurrentSquare,
            NextTurnOrder = match.CurrentTurnOrder,
            NextMoveNumber = nextMoveNumber,
            RemainingClockMilliseconds = mover.RemainingClockMilliseconds,
            IsMatchOver = isMatchOver,
        };
        IEnumerable<int> recipientAccountIds = match.Participations.Select(participation => participation.AccountId);
        var recipientAccountIdList = new List<int>(recipientAccountIds);
        _matchMoveNotifier.NotifyPawnMoved(match.MatchId, recipientAccountIdList, notice);
    }
}
