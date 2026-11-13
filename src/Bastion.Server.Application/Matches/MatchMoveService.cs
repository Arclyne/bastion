using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Matches;

/// <inheritdoc/>
public sealed class MatchMoveService : IMatchMoveService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchMoveService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchMoveNotifier _matchMoveNotifier;

    public MatchMoveService(
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
        EnsureTurn(match, player, move.MoveNumber);
        BoardCoordinates destination = ReadDestination(player, move);
        var pawnMove = new PawnMove(player.AccountId, move.MoveNumber, destination);
        byte boardSize = FindBoardSize(match);
        EnsureReachable(match, pawnMove, boardSize);
        Participation mover = match.GetParticipation(player.AccountId);
        string fromSquare = mover.CurrentSquare;
        match.MovePawn(pawnMove, boardSize, now);
        _matchRepository.SaveMove(match);
        _logger.Info(
            $"A pawn moved. AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={move.MoveNumber}");
        NotifyPawnMoved(match, player, fromSquare);
    }

    /// <inheritdoc/>
    public void PlaceWall(string sessionToken, ChosenWall chosenWall, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(chosenWall);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindPlayedMatch(player, chosenWall.MatchId);
        EnsureTurn(match, player, chosenWall.MoveNumber);
        Wall wall = ReadWall(player, chosenWall);
        var placement = new WallPlacement(player.AccountId, chosenWall.MoveNumber, wall);
        byte boardSize = FindBoardSize(match);
        EnsureWallFits(match, placement, boardSize);
        match.PlaceWall(placement, boardSize, now);
        _matchRepository.SaveMove(match);
        _logger.Info(
            "A wall was placed. "
            + $"AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={chosenWall.MoveNumber}");
        NotifyWallPlaced(match, player, wall);
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

    private static void EnsureTurn(Match match, ConnectedPlayer player, short moveNumber)
    {
        short nextMoveNumber = match.GetNextMoveNumber();
        if (match.IsTurnOf(player.AccountId) && moveNumber == nextMoveNumber)
        {
            return;
        }

        if (moveNumber > nextMoveNumber)
        {
            _logger.Warn(
                "A move arrived ahead of the expected number; the client may be modified. "
                + $"AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={moveNumber}; "
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

    private byte FindBoardSize(Match match)
    {
        GameMode? gameMode = _matchRepository.FindGameMode(match.GameModeId);
        if (gameMode is null)
        {
            throw new InvalidOperationException($"The game mode of the match does not exist. MatchId={match.MatchId}");
        }

        return gameMode.BoardSize;
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

    private void NotifyPawnMoved(Match match, ConnectedPlayer player, string fromSquare)
    {
        Participation mover = match.GetParticipation(player.AccountId);
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
        List<int> recipientAccountIds = ListParticipantAccountIds(match);
        _matchMoveNotifier.NotifyPawnMoved(match.MatchId, recipientAccountIds, notice);
    }

    private static List<int> ListParticipantAccountIds(Match match)
    {
        IEnumerable<int> participantAccountIds =
            match.Participations.Select(participation => participation.AccountId);
        return new List<int>(participantAccountIds);
    }

    private static Wall ReadWall(ConnectedPlayer player, ChosenWall chosenWall)
    {
        if (!BoardLayout.TryParseSquare(chosenWall.Groove, out BoardCoordinates groove))
        {
            _logger.Warn(
                $"A wall arrived with a groove outside board notation. AccountId={player.AccountId}; "
                + $"MatchId={chosenWall.MatchId}; Groove={chosenWall.Groove}");
            throw new IllegalWallException(WallRejectionReason.OutOfBoard, blockedTurnOrder: null);
        }

        return new Wall(groove, chosenWall.Orientation);
    }

    private static void EnsureWallFits(Match match, WallPlacement placement, byte boardSize)
    {
        WallRejection? rejection = match.FindWallRejection(placement, boardSize);
        if (rejection is null)
        {
            return;
        }

        string groove = BoardLayout.ToSquare(placement.Wall.Groove);
        string rejectionSentence = rejection.Reason == WallRejectionReason.NoWallsLeft
            ? "The server rejected a wall the interface does not allow; the client may be modified."
            : "The server rejected a wall.";
        _logger.Warn(
            $"{rejectionSentence} AccountId={placement.AccountId}; MatchId={match.MatchId}; Groove={groove}; "
            + $"Orientation={placement.Wall.Orientation}; Reason={rejection.Reason}");
        throw new IllegalWallException(rejection.Reason, rejection.BlockedTurnOrder);
    }

    private void NotifyWallPlaced(Match match, ConnectedPlayer player, Wall wall)
    {
        Participation placer = match.GetParticipation(player.AccountId);
        string groove = BoardLayout.ToSquare(wall.Groove);
        short nextMoveNumber = match.GetNextMoveNumber();
        var notice = new WallPlacementNotice
        {
            PlacerNickname = player.Nickname,
            Groove = groove,
            Orientation = wall.Orientation,
            RemainingWalls = placer.RemainingWalls,
            NextTurnOrder = match.CurrentTurnOrder,
            NextMoveNumber = nextMoveNumber,
            RemainingClockMilliseconds = placer.RemainingClockMilliseconds,
        };
        List<int> recipientAccountIds = ListParticipantAccountIds(match);
        _matchMoveNotifier.NotifyWallPlaced(match.MatchId, recipientAccountIds, notice);
    }
}
