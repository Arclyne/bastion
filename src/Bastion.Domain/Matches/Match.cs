using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Statistics;

namespace Bastion.Domain.Matches;

public class Match
{
    public const byte FirstTurnOrder = 1;
    public const byte SecondTurnOrder = 2;
    public const byte ThirdTurnOrder = 3;
    public const byte FourthTurnOrder = 4;
    public const int TwoPlayerCount = 2;
    public const short FirstMoveNumber = 1;

    private const int LastStandingPlayerCount = 1;
    private const int NotFoundIndex = -1;

    private readonly List<Participation> _participations;
    private readonly List<Move> _moves;
    private readonly List<DrawOffer> _drawOffers;

    private Match(MatchType matchType, MatchFormat format, DateTime startedAt)
        : this()
    {
        GameModeId = format.GameMode.GameModeId;
        MatchType = matchType;
        Status = MatchStatus.InProgress;
        ClockMinutes = format.ClockMinutes;
        StartedAt = startedAt;
        TurnStartedAt = startedAt + MatchClockPolicy.OpeningDelay;
        CurrentTurnOrder = FirstTurnOrder;
    }

    private Match()
    {
        _participations = [];
        _moves = [];
        _drawOffers = [];
    }

    private Match(Match source)
        : this()
    {
        MatchId = source.MatchId;
        GameModeId = source.GameModeId;
        MatchType = source.MatchType;
        Status = source.Status;
        ClockMinutes = source.ClockMinutes;
        StartedAt = source.StartedAt;
        TurnStartedAt = source.TurnStartedAt;
        EndedAt = source.EndedAt;
        EndReason = source.EndReason;
        CurrentTurnOrder = source.CurrentTurnOrder;
        IEnumerable<Participation> copiedParticipations = source._participations
            .Select(participation => participation.CopyForPreview());
        _participations.AddRange(copiedParticipations);
        _moves.AddRange(source._moves);
        _drawOffers.AddRange(source._drawOffers);
    }

    public int MatchId { get; private set; }

    public byte GameModeId { get; private set; }

    public MatchType MatchType { get; private set; }

    public MatchStatus Status { get; private set; }

    public byte ClockMinutes { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime TurnStartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public MatchEndReason? EndReason { get; private set; }

    public byte CurrentTurnOrder { get; private set; }

    public IReadOnlyCollection<Participation> Participations => _participations;

    public IReadOnlyCollection<Move> Moves => _moves;

    public IReadOnlyCollection<DrawOffer> DrawOffers => _drawOffers;

    public static Match StartRanked(
        MatchFormat format,
        IReadOnlyList<MatchEntrant> entrantsInTurnOrder,
        DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(entrantsInTurnOrder);

        var match = new Match(MatchType.Ranked, format, startedAt);
        match.SeatEntrants(entrantsInTurnOrder, format);
        return match;
    }

    public static Match StartPrivate(
        MatchFormat format,
        IReadOnlyList<MatchEntrant> entrantsInTurnOrder,
        DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(entrantsInTurnOrder);

        var match = new Match(MatchType.Private, format, startedAt);
        match.SeatEntrants(entrantsInTurnOrder, format);
        return match;
    }

    public bool CanBeAbandonedBy(int accountId)
    {
        return IsPlayedBy(accountId);
    }

    public bool IsPlayedBy(int accountId)
    {
        int participationIndex = FindActiveParticipationIndex(accountId);
        return participationIndex != NotFoundIndex;
    }

    public bool IsFinished()
    {
        return Status == MatchStatus.Finished;
    }

    public Participation GetParticipation(int accountId)
    {
        int participationIndex = FindParticipationIndex(accountId);
        if (participationIndex == NotFoundIndex)
        {
            throw new ArgumentOutOfRangeException(
                nameof(accountId),
                accountId,
                "The account does not play this match.");
        }

        return _participations[participationIndex];
    }

    public void Abandon(int abandoningAccountId, DateTime abandonedAt)
    {
        if (!CanBeAbandonedBy(abandoningAccountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can abandon it.");
        }

        if (IsTwoPlayerMatch())
        {
            EndTwoPlayerMatchByAbandonment(abandoningAccountId, abandonedAt);
            return;
        }

        LeaveFourPlayerMatch(abandoningAccountId, abandonedAt);
    }

    public void Disconnect(int accountId, DateTime disconnectedAt)
    {
        Participation disconnecting = GetActiveParticipation(accountId);
        disconnecting.Disconnect(disconnectedAt);
    }

    public void Reconnect(int accountId)
    {
        Participation reconnecting = GetActiveParticipation(accountId);
        reconnecting.Reconnect();
    }

    public bool HasAbsentPlayerToTakeOut(DateTime now)
    {
        if (Status != MatchStatus.InProgress)
        {
            return false;
        }

        List<Participation> activeParticipations = ListActiveParticipations();
        bool hasConnectedPlayer = activeParticipations.Exists(participation => participation.IsConnected);
        bool hasStayedAwayPlayer = activeParticipations.Exists(participation => participation.HasStayedAway(now));
        return hasConnectedPlayer && hasStayedAwayPlayer;
    }

    public int TakeOutAbsentPlayer(DateTime now)
    {
        if (!HasAbsentPlayerToTakeOut(now))
        {
            throw new InvalidOperationException("Only a player away for the whole return delay can be taken out.");
        }

        Participation absent = ListActiveParticipations()
            .Where(participation => participation.HasStayedAway(now))
            .OrderBy(participation => participation.DisconnectedAt)
            .First();
        DateTime disconnectedAt = absent.DisconnectedAt.GetValueOrDefault();
        DateTime takenOutAt = disconnectedAt + MatchConnectionPolicy.ReturnDelay;
        Abandon(absent.AccountId, takenOutAt);
        return absent.AccountId;
    }

    public void RecoverAfterRestart(DateTime restartedAt)
    {
        if (Status != MatchStatus.InProgress)
        {
            return;
        }

        foreach (Participation participation in ListActiveParticipations())
        {
            participation.Disconnect(restartedAt);
        }

        if (restartedAt > TurnStartedAt)
        {
            TurnStartedAt = restartedAt;
        }
    }

    public DrawOfferRejection FindDrawOfferRejection(int accountId)
    {
        if (!IsPlayedBy(accountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can offer a draw.");
        }

        if (!IsTwoPlayerMatch())
        {
            return new DrawOfferRejection(DrawOfferRejectionReason.NotTwoPlayers, 0);
        }

        if (HasPendingDrawOfferFrom(accountId))
        {
            return new DrawOfferRejection(DrawOfferRejectionReason.AlreadyOffered, 0);
        }

        int movesLeft = CountMovesBeforeNextDrawOffer(accountId);
        if (movesLeft > 0 && !HasPendingDrawOfferFor(accountId))
        {
            return new DrawOfferRejection(DrawOfferRejectionReason.TooSoon, movesLeft);
        }

        return DrawOfferRejection.None;
    }

    public DrawOfferOutcome OfferDraw(int accountId, DateTime offeredAt)
    {
        DrawOfferRejection rejection = FindDrawOfferRejection(accountId);
        if (rejection.Reason != DrawOfferRejectionReason.None)
        {
            throw new InvalidOperationException($"The draw cannot be offered. Reason={rejection.Reason}");
        }

        if (HasPendingDrawOfferFor(accountId))
        {
            AcceptDraw(accountId, offeredAt);
            return DrawOfferOutcome.Agreed;
        }

        short nextMoveNumber = GetNextMoveNumber();
        var drawOffer = new DrawOffer(accountId, nextMoveNumber, offeredAt);
        _drawOffers.Add(drawOffer);
        return DrawOfferOutcome.Offered;
    }

    public bool HasPendingDrawOfferFrom(int accountId)
    {
        return _drawOffers.Exists(drawOffer => drawOffer.IsPending() && drawOffer.OffererId == accountId);
    }

    public bool HasPendingDrawOfferFor(int accountId)
    {
        return _drawOffers.Exists(drawOffer => drawOffer.IsPending() && drawOffer.OffererId != accountId);
    }

    public int CountMovesBeforeNextDrawOffer(int accountId)
    {
        DrawOffer? lastOffer = _drawOffers
            .Where(drawOffer => drawOffer.OffererId == accountId)
            .OrderByDescending(drawOffer => drawOffer.MoveNumber)
            .FirstOrDefault();
        if (lastOffer is null)
        {
            return 0;
        }

        int ownMovesSinceOffer = _moves.Count(
            playedMove => playedMove.AccountId == accountId && playedMove.MoveNumber >= lastOffer.MoveNumber);
        return Math.Max(MatchDrawPolicy.OwnMovesBetweenOffers - ownMovesSinceOffer, 0);
    }

    public void AcceptDraw(int accountId, DateTime acceptedAt)
    {
        DrawOffer pendingOffer = GetPendingDrawOfferFor(accountId);
        pendingOffer.Accept(acceptedAt);
        foreach (Participation participation in _participations)
        {
            participation.Finish(ParticipationResult.Draw);
        }

        Finish(MatchEndReason.Draw, acceptedAt);
    }

    public void RejectDraw(int accountId, DateTime rejectedAt)
    {
        DrawOffer pendingOffer = GetPendingDrawOfferFor(accountId);
        pendingOffer.Reject(rejectedAt);
    }

    public AbandonmentPreview PreviewAbandonment(int abandoningAccountId, DateTime abandonedAt)
    {
        if (!CanBeAbandonedBy(abandoningAccountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can preview its abandonment.");
        }

        var abandonedCopy = new Match(this);
        abandonedCopy.Abandon(abandoningAccountId, abandonedAt);
        if (abandonedCopy.CanBeRated())
        {
            abandonedCopy.Rate();
        }

        Participation leaver = abandonedCopy.GetParticipation(abandoningAccountId);
        int activePlayerCount = abandonedCopy.CountActivePlayers();
        bool isRanked = MatchType == MatchType.Ranked;
        return new AbandonmentPreview
        {
            IsRanked = isRanked,
            InitialElo = leaver.InitialElo,
            FinalElo = leaver.FinalElo,
            Placement = leaver.Placement,
            ActivePlayerCount = activePlayerCount,
        };
    }

    public Participation GetTurnParticipation()
    {
        int participationIndex = _participations.FindIndex(
            participation => participation.TurnOrder == CurrentTurnOrder);
        if (participationIndex == NotFoundIndex)
        {
            throw new InvalidOperationException("No participation has the current turn.");
        }

        return _participations[participationIndex];
    }

    public bool HasClockRunOut(DateTime now)
    {
        if (Status != MatchStatus.InProgress)
        {
            return false;
        }

        Participation turnParticipation = GetTurnParticipation();
        if (turnParticipation.RemainingClockMilliseconds is not int remainingMilliseconds)
        {
            return false;
        }

        TimeSpan elapsedTime = now - TurnStartedAt;
        return elapsedTime.TotalMilliseconds >= remainingMilliseconds;
    }

    public void ExpireClock(DateTime now)
    {
        if (!HasClockRunOut(now))
        {
            throw new InvalidOperationException("Only a clock that has run out can expire.");
        }

        Participation timedOut = GetTurnParticipation();
        int remainingMilliseconds = timedOut.RemainingClockMilliseconds.GetValueOrDefault();
        DateTime ranOutAt = TurnStartedAt.AddMilliseconds(remainingMilliseconds);
        timedOut.RunOutOfClock();
        if (IsTwoPlayerMatch())
        {
            Participation rival = _participations.First(participation => participation.AccountId != timedOut.AccountId);
            FinishTwoPlayerMatch(rival.AccountId, MatchEndReason.Timeout, ranOutAt);
            return;
        }

        byte placement = FindWorstFreePlacement();
        timedOut.Leave(MatchEndReason.Timeout, placement);
        EndOrPassTurnAfter(timedOut, MatchEndReason.Timeout, ranOutAt);
    }

    public short GetNextMoveNumber()
    {
        int nextMoveNumber = _moves.Count + FirstMoveNumber;
        short moveNumber = (short)nextMoveNumber;
        return moveNumber;
    }

    public bool IsTurnOf(int accountId)
    {
        int participationIndex = FindActiveParticipationIndex(accountId);
        if (participationIndex == NotFoundIndex)
        {
            return false;
        }

        Participation participation = _participations[participationIndex];
        return participation.TurnOrder == CurrentTurnOrder;
    }

    public Board BuildBoard(byte boardSize)
    {
        List<Participation> activeParticipations = ListActiveParticipations();
        var pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>(activeParticipations.Count);
        foreach (Participation participation in activeParticipations)
        {
            pawnsByTurnOrder[participation.TurnOrder] = FindPawnCoordinates(participation, boardSize);
        }

        List<Wall> walls = ListPlacedWalls();
        return new Board(boardSize, pawnsByTurnOrder, walls);
    }

    public IReadOnlyList<BoardCoordinates> ListReachableSquares(int accountId, byte boardSize)
    {
        int participationIndex = FindActiveParticipationIndex(accountId);
        if (participationIndex == NotFoundIndex)
        {
            return [];
        }

        Participation participation = _participations[participationIndex];
        Board board = BuildBoard(boardSize);
        IReadOnlyList<BoardCoordinates> reachableSquares = PawnMovePolicy.ListReachableSquares(
            board,
            participation.TurnOrder);
        return reachableSquares;
    }

    public void MovePawn(PawnMove pawnMove, byte boardSize, DateTime playedAt)
    {
        ArgumentNullException.ThrowIfNull(pawnMove);

        EnsureClockLeft(playedAt);
        EnsurePawnCanMove(pawnMove, boardSize);
        Participation mover = GetParticipation(pawnMove.AccountId);
        MoveTiming timing = MeasureTurn(playedAt);
        var playedMove = new Move(pawnMove, mover.CurrentSquare, timing);
        RecordMove(mover, playedMove);
        string destinationSquare = BoardLayout.ToSquare(pawnMove.Destination);
        mover.MoveTo(destinationSquare);
        bool hasReachedGoal = BoardLayout.IsGoal(boardSize, mover.TurnOrder, pawnMove.Destination);
        if (hasReachedGoal && IsTwoPlayerMatch())
        {
            FinishTwoPlayerMatch(mover.AccountId, MatchEndReason.Goal, playedAt);
            return;
        }

        if (hasReachedGoal)
        {
            ClassifyAtGoal(mover, playedAt);
            return;
        }

        PassTurnFrom(mover.TurnOrder, playedAt);
    }

    public bool CanBeRated()
    {
        bool isFinishedRankedDuel = MatchType == MatchType.Ranked && IsFinished() && IsTwoPlayerMatch();
        return isFinishedRankedDuel && _participations.All(participation => participation.InitialElo.HasValue);
    }

    public void Rate()
    {
        if (!CanBeRated())
        {
            throw new InvalidOperationException(
                "Only a finished two-player ranked match whose players have an initial elo can be rated.");
        }

        foreach (Participation participation in _participations)
        {
            RateParticipation(participation);
        }
    }

    public bool HasAbandoned(int accountId)
    {
        Participation participation = GetParticipation(accountId);
        if (participation.EndReason == MatchEndReason.Abandonment)
        {
            return true;
        }

        bool isLostDuel = IsTwoPlayerMatch() && participation.Result == ParticipationResult.Lost;
        return isLostDuel && EndReason == MatchEndReason.Abandonment;
    }

    public int CountActivePlayers()
    {
        List<Participation> activeParticipations = ListActiveParticipations();
        return activeParticipations.Count;
    }

    public WallRejection FindWallRejection(WallPlacement placement, byte boardSize)
    {
        ArgumentNullException.ThrowIfNull(placement);

        int placerIndex = FindActiveParticipationIndex(placement.AccountId);
        if (placerIndex == NotFoundIndex)
        {
            throw new InvalidOperationException("Only a player of a match in progress can place a wall.");
        }

        Participation placer = _participations[placerIndex];
        if (!placer.HasWallsLeft())
        {
            return new WallRejection(WallRejectionReason.NoWallsLeft, BlockedTurnOrder: null);
        }

        Board board = BuildBoard(boardSize);
        WallRejection rejection = WallPolicy.FindRejection(board, placement.Wall);
        return rejection;
    }

    public void PlaceWall(WallPlacement placement, byte boardSize, DateTime playedAt)
    {
        ArgumentNullException.ThrowIfNull(placement);

        EnsureClockLeft(playedAt);
        EnsureExpectedMove(placement.AccountId, placement.MoveNumber);
        WallRejection rejection = FindWallRejection(placement, boardSize);
        if (rejection.Reason != WallRejectionReason.None)
        {
            throw new InvalidOperationException($"The wall cannot be placed. Reason={rejection.Reason}");
        }

        Participation placer = GetParticipation(placement.AccountId);
        MoveTiming timing = MeasureTurn(playedAt);
        var playedMove = new Move(placement, timing);
        RecordMove(placer, playedMove);
        placer.SpendWall();
        PassTurnFrom(placer.TurnOrder, playedAt);
    }

    private void SeatEntrants(IReadOnlyList<MatchEntrant> entrantsInTurnOrder, MatchFormat format)
    {
        if (entrantsInTurnOrder.Count != format.GameMode.PlayerCount)
        {
            throw new ArgumentException(
                "The number of entrants differs from the player count of the game mode.",
                nameof(entrantsInTurnOrder));
        }

        for (int entrantIndex = 0; entrantIndex < entrantsInTurnOrder.Count; entrantIndex++)
        {
            int turnOrderNumber = FirstTurnOrder + entrantIndex;
            byte turnOrder = (byte)turnOrderNumber;
            MatchEntrant entrant = entrantsInTurnOrder[entrantIndex];
            var participation = new Participation(entrant, turnOrder, format);
            _participations.Add(participation);
        }
    }

    private int FindActiveParticipationIndex(int accountId)
    {
        if (Status != MatchStatus.InProgress)
        {
            return NotFoundIndex;
        }

        int participationIndex = FindParticipationIndex(accountId);
        if (participationIndex == NotFoundIndex || _participations[participationIndex].HasEnded())
        {
            return NotFoundIndex;
        }

        return participationIndex;
    }

    private Participation GetActiveParticipation(int accountId)
    {
        int participationIndex = FindActiveParticipationIndex(accountId);
        if (participationIndex == NotFoundIndex)
        {
            throw new InvalidOperationException("Only a player of a match in progress can change connection.");
        }

        return _participations[participationIndex];
    }

    private int FindParticipationIndex(int accountId)
    {
        return _participations.FindIndex(participation => participation.AccountId == accountId);
    }

    private bool IsTwoPlayerMatch()
    {
        return _participations.Count == TwoPlayerCount;
    }

    private void EndTwoPlayerMatchByAbandonment(int abandoningAccountId, DateTime endedAt)
    {
        Participation rival = _participations.First(participation => participation.AccountId != abandoningAccountId);
        FinishTwoPlayerMatch(rival.AccountId, MatchEndReason.Abandonment, endedAt);
    }

    private void FinishTwoPlayerMatch(int winnerAccountId, MatchEndReason endReason, DateTime endedAt)
    {
        foreach (Participation participation in _participations)
        {
            bool isWinner = participation.AccountId == winnerAccountId;
            participation.Finish(isWinner ? ParticipationResult.Won : ParticipationResult.Lost);
        }

        Finish(endReason, endedAt);
    }

    private void Finish(MatchEndReason endReason, DateTime endedAt)
    {
        ExpirePendingDrawOffers(endedAt);
        Status = MatchStatus.Finished;
        EndedAt = endedAt;
        EndReason = endReason;
    }

    private void LeaveFourPlayerMatch(int abandoningAccountId, DateTime abandonedAt)
    {
        Participation abandoning = GetParticipation(abandoningAccountId);
        byte placement = FindWorstFreePlacement();
        abandoning.Leave(MatchEndReason.Abandonment, placement);
        EndOrPassTurnAfter(abandoning, MatchEndReason.Abandonment, abandonedAt);
    }

    private void ClassifyAtGoal(Participation arriving, DateTime arrivedAt)
    {
        byte placement = FindBestFreePlacement();
        arriving.ReachGoal(placement);
        EndOrPassTurnAfter(arriving, MatchEndReason.Goal, arrivedAt);
    }

    private void EndOrPassTurnAfter(Participation departing, MatchEndReason endReason, DateTime departedAt)
    {
        List<Participation> activeParticipations = ListActiveParticipations();
        if (activeParticipations.Count > LastStandingPlayerCount)
        {
            PassTurnFrom(departing.TurnOrder, departedAt);
            return;
        }

        Participation lastStanding = activeParticipations.Single();
        byte placement = FindBestFreePlacement();
        lastStanding.TakePlacement(placement);
        Finish(endReason, departedAt);
    }

    private byte FindBestFreePlacement()
    {
        int lastPlacement = _participations.Count;
        for (int placementNumber = Participation.FirstPlacement; placementNumber <= lastPlacement; placementNumber++)
        {
            byte placement = (byte)placementNumber;
            if (!IsPlacementTaken(placement))
            {
                return placement;
            }
        }

        throw new InvalidOperationException("Every placement of the match is taken.");
    }

    private byte FindWorstFreePlacement()
    {
        int lastPlacement = _participations.Count;
        for (int placementNumber = lastPlacement; placementNumber >= Participation.FirstPlacement; placementNumber--)
        {
            byte placement = (byte)placementNumber;
            if (!IsPlacementTaken(placement))
            {
                return placement;
            }
        }

        throw new InvalidOperationException("Every placement of the match is taken.");
    }

    private bool IsPlacementTaken(byte placement)
    {
        return _participations.Any(participation => participation.Placement == placement);
    }

    private void RateParticipation(Participation participation)
    {
        Participation rival = _participations.First(candidate => candidate.AccountId != participation.AccountId);
        short ownElo = participation.InitialElo.GetValueOrDefault();
        short rivalElo = rival.InitialElo.GetValueOrDefault();
        ParticipationResult result = participation.Result.GetValueOrDefault();
        short finalElo = EloPolicy.CalculateFinalElo(ownElo, rivalElo, result);
        participation.RecordFinalElo(finalElo);
    }

    private void PassTurnFrom(byte turnOrder, DateTime turnEndedAt)
    {
        if (CurrentTurnOrder != turnOrder)
        {
            return;
        }

        TurnStartedAt = turnEndedAt;

        List<Participation> activeParticipations = ListActiveParticipations();
        Participation nextPlayer = activeParticipations.FirstOrDefault(
            participation => participation.TurnOrder > turnOrder)
            ?? activeParticipations.First();
        CurrentTurnOrder = nextPlayer.TurnOrder;
    }

    private List<Participation> ListActiveParticipations()
    {
        IEnumerable<Participation> activeParticipations = _participations
            .Where(participation => !participation.HasEnded())
            .OrderBy(participation => participation.TurnOrder);
        return [.. activeParticipations];
    }

    private BoardCoordinates FindPawnCoordinates(Participation participation, byte boardSize)
    {
        Move? lastPawnMove = _moves
            .Where(playedMove => playedMove.AccountId == participation.AccountId
                && playedMove.MoveType == MoveType.PawnMove)
            .OrderBy(playedMove => playedMove.MoveNumber)
            .LastOrDefault();
        if (lastPawnMove is null)
        {
            BoardCoordinates startingCoordinates = BoardLayout.GetStartingCoordinates(
                boardSize,
                participation.TurnOrder);
            return startingCoordinates;
        }

        BoardCoordinates destination = lastPawnMove.GetDestination();
        return destination;
    }

    private List<Wall> ListPlacedWalls()
    {
        IEnumerable<Wall> placedWalls = _moves
            .Where(playedMove => playedMove.MoveType == MoveType.Wall)
            .Select(playedMove => playedMove.GetWall());
        return [.. placedWalls];
    }

    private void EnsureClockLeft(DateTime playedAt)
    {
        if (HasClockRunOut(playedAt))
        {
            throw new InvalidOperationException("The clock of the player who has the turn ran out.");
        }
    }

    private void EnsurePawnCanMove(PawnMove pawnMove, byte boardSize)
    {
        EnsureExpectedMove(pawnMove.AccountId, pawnMove.MoveNumber);
        IReadOnlyList<BoardCoordinates> reachableSquares = ListReachableSquares(pawnMove.AccountId, boardSize);
        if (!reachableSquares.Contains(pawnMove.Destination))
        {
            throw new InvalidOperationException("The pawn cannot reach that square.");
        }
    }

    private void EnsureExpectedMove(int accountId, short moveNumber)
    {
        if (!IsTurnOf(accountId) || moveNumber != GetNextMoveNumber())
        {
            throw new InvalidOperationException("Only the player who has the turn can make the expected move.");
        }
    }

    private MoveTiming MeasureTurn(DateTime playedAt)
    {
        int timeSpentMilliseconds = GetTimeSpentMilliseconds(playedAt);
        return new MoveTiming(timeSpentMilliseconds, playedAt);
    }

    private int GetTimeSpentMilliseconds(DateTime playedAt)
    {
        TimeSpan elapsedTime = playedAt - TurnStartedAt;
        double elapsedMilliseconds = Math.Max(elapsedTime.TotalMilliseconds, 0);
        int timeSpentMilliseconds = (int)elapsedMilliseconds;
        return timeSpentMilliseconds;
    }

    private void RecordMove(Participation player, Move playedMove)
    {
        _moves.Add(playedMove);
        player.SpendClock(playedMove.TimeSpentMilliseconds);
        IEnumerable<DrawOffer> offersToPlayer = _drawOffers.Where(
            drawOffer => drawOffer.IsPending() && drawOffer.OffererId != player.AccountId);
        foreach (DrawOffer offerToPlayer in offersToPlayer)
        {
            offerToPlayer.Expire(playedMove.PlayedAt);
        }
    }

    private DrawOffer GetPendingDrawOfferFor(int accountId)
    {
        if (!IsPlayedBy(accountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can answer a draw offer.");
        }

        DrawOffer? pendingOffer = _drawOffers.Find(
            drawOffer => drawOffer.IsPending() && drawOffer.OffererId != accountId);
        return pendingOffer ?? throw new InvalidOperationException("There is no draw offer to answer.");
    }

    private void ExpirePendingDrawOffers(DateTime expiredAt)
    {
        IEnumerable<DrawOffer> pendingOffers = _drawOffers.Where(drawOffer => drawOffer.IsPending());
        foreach (DrawOffer pendingOffer in pendingOffers)
        {
            pendingOffer.Expire(expiredAt);
        }
    }
}
