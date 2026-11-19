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
public sealed class MatchMoveService : IMatchMoveService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchMoveService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchMoveNotifier _matchMoveNotifier;
    private readonly IMatchCloser _matchCloser;
    private readonly IMatchDrawNotifier _drawNotifier;

    public MatchMoveService(
        ISessionVerifier sessionVerifier,
        IMatchRepository matchRepository,
        MatchMoveFollowUp followUp)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(followUp);

        _sessionVerifier = sessionVerifier;
        _matchRepository = matchRepository;
        _matchMoveNotifier = followUp.MoveNotifier;
        _matchCloser = followUp.Closer;
        _drawNotifier = followUp.DrawNotifier;
    }

    /// <inheritdoc/>
    public void MovePawn(string sessionToken, ChosenPawnMove chosenMove, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(chosenMove);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindPlayedMatch(player, chosenMove.MatchId);
        ExpireRunOutClock(match, player, now);
        EnsureTurn(match, player, chosenMove.MoveNumber);
        BoardCoordinates destination = ReadDestination(player, chosenMove);
        var pawnMove = new PawnMove(player.AccountId, chosenMove.MoveNumber, destination);
        byte boardSize = FindBoardSize(match);
        EnsureReachable(match, pawnMove, boardSize);
        Participation mover = match.GetParticipation(player.AccountId);
        string fromSquare = mover.CurrentSquare;
        bool hadDrawOffer = match.HasPendingDrawOfferFor(player.AccountId);
        match.MovePawn(pawnMove, boardSize, now);
        SaveAfterPawnMove(match);
        _logger.Info(
            $"A pawn moved. AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={chosenMove.MoveNumber}");
        NotifyPawnMoved(match, player, fromSquare);
        NotifyExpiredDrawOffer(match, hadDrawOffer);
        AnnounceAfterPawnMove(match, player.AccountId, now);
    }

    /// <inheritdoc/>
    public void PlaceWall(string sessionToken, ChosenWall chosenWall, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(chosenWall);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        Match match = FindPlayedMatch(player, chosenWall.MatchId);
        ExpireRunOutClock(match, player, now);
        EnsureTurn(match, player, chosenWall.MoveNumber);
        Wall wall = ReadWall(player, chosenWall);
        var placement = new WallPlacement(player.AccountId, chosenWall.MoveNumber, wall);
        byte boardSize = FindBoardSize(match);
        EnsureWallFits(match, placement, boardSize);
        bool hadDrawOffer = match.HasPendingDrawOfferFor(player.AccountId);
        match.PlaceWall(placement, boardSize, now);
        _matchRepository.SaveMove(match);
        _logger.Info(
            "A wall was placed. "
            + $"AccountId={player.AccountId}; MatchId={match.MatchId}; MoveNumber={chosenWall.MoveNumber}");
        NotifyWallPlaced(match, player, wall);
        NotifyExpiredDrawOffer(match, hadDrawOffer);
    }

    private Match FindPlayedMatch(ConnectedPlayer player, int matchId)
    {
        Lookup<Match> foundMatch = _matchRepository.FindMatch(matchId);
        if (!foundMatch.IsFound || !foundMatch.GetValue().IsPlayedBy(player.AccountId))
        {
            _logger.Warn(
                "A player without an active participation tried to move in the match. "
                + $"AccountId={player.AccountId}; MatchId={matchId}");
            throw new MatchNotJoinedException();
        }

        return foundMatch.GetValue();
    }

    private void ExpireRunOutClock(Match match, ConnectedPlayer player, DateTime now)
    {
        if (!match.HasClockRunOut(now))
        {
            return;
        }

        _matchCloser.ExpireClock(match, now);
        if (match.IsPlayedBy(player.AccountId))
        {
            return;
        }

        _logger.Info(
            "A move arrived after the clock of the player ran out. "
            + $"AccountId={player.AccountId}; MatchId={match.MatchId}");
        throw new MatchNotJoinedException();
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

    private static BoardCoordinates ReadDestination(ConnectedPlayer player, ChosenPawnMove chosenMove)
    {
        BoardCoordinates destination = BoardLayout.FindSquare(chosenMove.DestinationSquare);
        if (destination == BoardLayout.OffBoardSquare)
        {
            _logger.Warn(
                $"A move arrived with a square outside board notation. AccountId={player.AccountId}; "
                + $"MatchId={chosenMove.MatchId}; Square={chosenMove.DestinationSquare}");
            throw new IllegalMoveException();
        }

        return destination;
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

    private void SaveAfterPawnMove(Match match)
    {
        if (match.IsFinished())
        {
            _matchCloser.CloseAfterMove(match);
            return;
        }

        _matchRepository.SaveMove(match);
    }

    private void NotifyPawnMoved(Match match, ConnectedPlayer player, string fromSquare)
    {
        Participation mover = match.GetParticipation(player.AccountId);
        short nextMoveNumber = match.GetNextMoveNumber();
        var notice = new PawnMoveNotice
        {
            MoverNickname = player.Nickname,
            FromSquare = fromSquare,
            ToSquare = mover.CurrentSquare,
            NextTurnOrder = match.CurrentTurnOrder,
            NextMoveNumber = nextMoveNumber,
            RemainingClockMilliseconds = mover.RemainingClockMilliseconds,
        };
        List<int> recipientAccountIds = ListParticipantAccountIds(match);
        _matchMoveNotifier.NotifyPawnMoved(match.MatchId, recipientAccountIds, notice);
    }

    private static List<int> ListParticipantAccountIds(Match match)
    {
        IEnumerable<int> participantAccountIds = match.Participations.Select(participation => participation.AccountId);
        return [.. participantAccountIds];
    }

    private void NotifyExpiredDrawOffer(Match match, bool hadDrawOffer)
    {
        if (!hadDrawOffer || match.IsFinished())
        {
            return;
        }

        List<int> recipientAccountIds = ListParticipantAccountIds(match);
        _drawNotifier.NotifyDrawExpired(match.MatchId, recipientAccountIds);
    }

    private void AnnounceAfterPawnMove(Match match, int moverAccountId, DateTime now)
    {
        if (match.IsFinished())
        {
            _matchCloser.AnnounceEnd(match, moverAccountId);
            return;
        }

        Participation mover = match.GetParticipation(moverAccountId);
        if (mover.HasEnded())
        {
            _matchCloser.AnnounceDeparture(match, moverAccountId, now);
        }
    }

    private static Wall ReadWall(ConnectedPlayer player, ChosenWall chosenWall)
    {
        BoardCoordinates groove = BoardLayout.FindSquare(chosenWall.Groove);
        if (groove == BoardLayout.OffBoardSquare)
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
        WallRejection rejection = match.FindWallRejection(placement, boardSize);
        if (rejection.Reason == WallRejectionReason.None)
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
