using System;
using System.Collections.Generic;
using System.Linq;

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
    private const byte FirstPlacement = 1;
    private const double NoTimeSpent = 0;

    private readonly List<Participation> _participations = new List<Participation>();
    private readonly List<Move> _moves = new List<Move>();

    private Match(MatchType matchType, MatchFormat format, DateTime startedAt)
    {
        GameModeId = format.GameMode.GameModeId;
        MatchType = matchType;
        Status = MatchStatus.InProgress;
        ClockMinutes = format.ClockMinutes;
        StartedAt = startedAt;
        CurrentTurnOrder = FirstTurnOrder;
    }

    private Match()
    {
    }

    public int MatchId { get; private set; }

    public byte GameModeId { get; private set; }

    public MatchType MatchType { get; private set; }

    public MatchStatus Status { get; private set; }

    public byte ClockMinutes { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public MatchEndReason? EndReason { get; private set; }

    public byte CurrentTurnOrder { get; private set; }

    public IReadOnlyCollection<Participation> Participations => _participations;

    public IReadOnlyCollection<Move> Moves => _moves;

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
        Participation? participation = FindActiveParticipation(accountId);
        return participation is not null;
    }

    public void Abandon(int abandoningAccountId, DateTime abandonedAt)
    {
        if (!CanBeAbandonedBy(abandoningAccountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can abandon it.");
        }

        if (_participations.Count == TwoPlayerCount)
        {
            EndTwoPlayerMatchByAbandonment(abandoningAccountId, abandonedAt);
            return;
        }

        LeaveFourPlayerMatch(abandoningAccountId, abandonedAt);
    }

    public short GetNextMoveNumber()
    {
        int nextMoveNumber = _moves.Count + FirstMoveNumber;
        short moveNumber = (short)nextMoveNumber;
        return moveNumber;
    }

    public bool IsTurnOf(int accountId)
    {
        Participation? participation = FindActiveParticipation(accountId);
        return participation is not null && participation.TurnOrder == CurrentTurnOrder;
    }

    public Board BuildBoard(byte boardSize)
    {
        var pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>();
        foreach (Participation participation in ListActiveParticipations())
        {
            pawnsByTurnOrder[participation.TurnOrder] = FindPawnCoordinates(participation, boardSize);
        }

        List<Wall> walls = ListPlacedWalls();
        return new Board(boardSize, pawnsByTurnOrder, walls);
    }

    public IReadOnlyList<BoardCoordinates> ListReachableSquares(int accountId, byte boardSize)
    {
        Participation? participation = FindActiveParticipation(accountId);
        if (participation is null)
        {
            return new List<BoardCoordinates>();
        }

        Board board = BuildBoard(boardSize);
        IReadOnlyList<BoardCoordinates> reachableSquares =
            PawnMovePolicy.ListReachableSquares(board, participation.TurnOrder);
        return reachableSquares;
    }

    public void MovePawn(PawnMove pawnMove, byte boardSize, DateTime playedAt)
    {
        ArgumentNullException.ThrowIfNull(pawnMove);

        EnsurePawnCanMove(pawnMove, boardSize);
        Participation mover = _participations.First(participation => participation.AccountId == pawnMove.AccountId);
        MoveTiming timing = MeasureTurn(playedAt);
        var move = new Move(pawnMove, mover.CurrentSquare, timing);
        RecordMove(mover, move);
        string destinationSquare = BoardLayout.ToSquare(pawnMove.Destination);
        mover.MoveTo(destinationSquare);
        bool hasReachedGoal = BoardLayout.IsGoal(boardSize, mover.TurnOrder, pawnMove.Destination);
        if (hasReachedGoal && _participations.Count == TwoPlayerCount)
        {
            EndByGoal(mover, playedAt);
            return;
        }

        PassTurnFrom(mover.TurnOrder);
    }

    public WallRejection? FindWallRejection(WallPlacement placement, byte boardSize)
    {
        ArgumentNullException.ThrowIfNull(placement);

        Participation? placer = FindActiveParticipation(placement.AccountId);
        if (placer is null)
        {
            throw new InvalidOperationException("Only a player of a match in progress can place a wall.");
        }

        if (!placer.HasWallsLeft())
        {
            return new WallRejection(WallRejectionReason.NoWallsLeft, BlockedTurnOrder: null);
        }

        Board board = BuildBoard(boardSize);
        WallRejection? rejection = WallPolicy.FindRejection(board, placement.Wall);
        return rejection;
    }

    public void PlaceWall(WallPlacement placement, byte boardSize, DateTime playedAt)
    {
        ArgumentNullException.ThrowIfNull(placement);

        EnsureExpectedMove(placement.AccountId, placement.MoveNumber);
        WallRejection? rejection = FindWallRejection(placement, boardSize);
        if (rejection is not null)
        {
            throw new InvalidOperationException($"The wall cannot be placed. Reason={rejection.Reason}");
        }

        Participation placer = _participations.First(participation => participation.AccountId == placement.AccountId);
        MoveTiming timing = MeasureTurn(playedAt);
        var move = new Move(placement, timing);
        RecordMove(placer, move);
        placer.SpendWall();
        PassTurnFrom(placer.TurnOrder);
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

    private Participation? FindActiveParticipation(int accountId)
    {
        Participation? participation = FindParticipation(accountId);
        if (Status != MatchStatus.InProgress || participation is null || participation.Result.HasValue)
        {
            return null;
        }

        return participation;
    }

    private Participation? FindParticipation(int accountId)
    {
        return _participations.FirstOrDefault(participation => participation.AccountId == accountId);
    }

    private void EndTwoPlayerMatchByAbandonment(int abandoningAccountId, DateTime endedAt)
    {
        foreach (Participation participation in _participations)
        {
            bool isAbandoning = participation.AccountId == abandoningAccountId;
            participation.Finish(isAbandoning ? ParticipationResult.Lost : ParticipationResult.Won);
        }

        Finish(MatchEndReason.Abandonment, endedAt);
    }

    private void Finish(MatchEndReason endReason, DateTime endedAt)
    {
        Status = MatchStatus.Finished;
        EndedAt = endedAt;
        EndReason = endReason;
    }

    private void LeaveFourPlayerMatch(int abandoningAccountId, DateTime abandonedAt)
    {
        List<Participation> activeParticipations = ListActiveParticipations();
        Participation abandoning =
            activeParticipations.First(participation => participation.AccountId == abandoningAccountId);
        byte placement = (byte)activeParticipations.Count;
        abandoning.Leave(MatchEndReason.Abandonment, placement);
        activeParticipations.Remove(abandoning);
        if (activeParticipations.Count > LastStandingPlayerCount)
        {
            PassTurnFrom(abandoning.TurnOrder);
            return;
        }

        activeParticipations.Single().Win(FirstPlacement);
        Finish(MatchEndReason.Abandonment, abandonedAt);
    }

    private List<Participation> ListActiveParticipations()
    {
        IEnumerable<Participation> activeParticipations = _participations
            .Where(participation => !participation.Result.HasValue)
            .OrderBy(participation => participation.TurnOrder);
        return new List<Participation>(activeParticipations);
    }

    private void PassTurnFrom(byte leavingTurnOrder)
    {
        if (CurrentTurnOrder != leavingTurnOrder)
        {
            return;
        }

        List<Participation> activeParticipations = ListActiveParticipations();
        Participation nextPlayer =
            activeParticipations.FirstOrDefault(participation => participation.TurnOrder > leavingTurnOrder)
            ?? activeParticipations.First();
        CurrentTurnOrder = nextPlayer.TurnOrder;
    }

    private BoardCoordinates FindPawnCoordinates(Participation participation, byte boardSize)
    {
        Move? lastPawnMove = _moves
            .Where(move => move.AccountId == participation.AccountId && move.MoveType == MoveType.PawnMove)
            .OrderBy(move => move.MoveNumber)
            .LastOrDefault();
        if (lastPawnMove is null)
        {
            BoardCoordinates startingCoordinates =
                BoardLayout.GetStartingCoordinates(boardSize, participation.TurnOrder);
            return startingCoordinates;
        }

        BoardCoordinates destination = lastPawnMove.GetDestination();
        return destination;
    }

    private List<Wall> ListPlacedWalls()
    {
        IEnumerable<Wall> placedWalls = _moves
            .Where(move => move.MoveType == MoveType.Wall)
            .Select(move => move.GetWall());
        return new List<Wall>(placedWalls);
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
        DateTime turnStartedAt = _moves.Count == 0 ? StartedAt : _moves.Max(move => move.PlayedAt);
        TimeSpan elapsedTime = playedAt - turnStartedAt;
        double elapsedMilliseconds = Math.Max(elapsedTime.TotalMilliseconds, NoTimeSpent);
        int timeSpentMilliseconds = (int)elapsedMilliseconds;
        return timeSpentMilliseconds;
    }

    private void RecordMove(Participation player, Move move)
    {
        _moves.Add(move);
        player.SpendClock(move.TimeSpentMilliseconds);
    }

    private void EndByGoal(Participation winner, DateTime endedAt)
    {
        foreach (Participation participation in _participations)
        {
            bool isWinner = participation.AccountId == winner.AccountId;
            participation.Finish(isWinner ? ParticipationResult.Won : ParticipationResult.Lost);
        }

        Finish(MatchEndReason.Goal, endedAt);
    }
}
