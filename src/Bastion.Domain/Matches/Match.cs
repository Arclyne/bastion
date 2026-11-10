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

    private const int TwoPlayers = 2;
    private const int LastStandingPlayerCount = 1;
    private const byte FirstPlacement = 1;

    private readonly List<Participation> _participations = new List<Participation>();

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
        Participation? participation = FindParticipation(accountId);
        return Status == MatchStatus.InProgress && participation is not null && !participation.Result.HasValue;
    }

    public void Abandon(int abandoningAccountId, DateTime abandonedAt)
    {
        if (!CanBeAbandonedBy(abandoningAccountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can abandon it.");
        }

        if (_participations.Count == TwoPlayers)
        {
            EndTwoPlayerMatchByAbandonment(abandoningAccountId, abandonedAt);
            return;
        }

        LeaveFourPlayerMatch(abandoningAccountId, abandonedAt);
    }

    private void EndTwoPlayerMatchByAbandonment(int abandoningAccountId, DateTime endedAt)
    {
        foreach (Participation participation in _participations)
        {
            bool isAbandoning = participation.AccountId == abandoningAccountId;
            participation.Finish(isAbandoning ? ParticipationResult.Lost : ParticipationResult.Won);
        }

        EndByAbandonment(endedAt);
    }

    private void LeaveFourPlayerMatch(int abandoningAccountId, DateTime abandonedAt)
    {
        List<Participation> activeParticipations = ListActiveParticipations();
        Participation abandoning =
            activeParticipations.First(participation => participation.AccountId == abandoningAccountId);
        abandoning.Leave(MatchEndReason.Abandonment, (byte)activeParticipations.Count);
        activeParticipations.Remove(abandoning);
        if (activeParticipations.Count > LastStandingPlayerCount)
        {
            PassTurnFrom(abandoning.TurnOrder);
            return;
        }

        activeParticipations.Single().Win(FirstPlacement);
        EndByAbandonment(abandonedAt);
    }

    private void EndByAbandonment(DateTime endedAt)
    {
        Status = MatchStatus.Finished;
        EndedAt = endedAt;
        EndReason = MatchEndReason.Abandonment;
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

    private List<Participation> ListActiveParticipations()
    {
        IEnumerable<Participation> activeParticipations = _participations
            .Where(participation => !participation.Result.HasValue)
            .OrderBy(participation => participation.TurnOrder);
        return new List<Participation>(activeParticipations);
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
            byte turnOrder = (byte)(FirstTurnOrder + entrantIndex);
            _participations.Add(new Participation(entrantsInTurnOrder[entrantIndex], turnOrder, format));
        }
    }

    private Participation? FindParticipation(int accountId)
    {
        return _participations.FirstOrDefault(participation => participation.AccountId == accountId);
    }
}
