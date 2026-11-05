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

    // Entity Framework Core defines the navigation by this property (rule 2.11.4).
    public IReadOnlyCollection<Participation> Participations => _participations;

    public static Match StartRanked(
        MatchFormat format,
        IReadOnlyList<MatchEntrant> entrantsInTurnOrder,
        DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(format);

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

        var match = new Match(MatchType.Private, format, startedAt);
        match.SeatEntrants(entrantsInTurnOrder, format);
        return match;
    }

    public bool CanBeAbandonedBy(int accountId)
    {
        Participation? participation = FindParticipation(accountId);
        return Status == MatchStatus.InProgress && participation is not null && !participation.Result.HasValue;
    }

    public void EndByAbandonment(int abandoningAccountId, DateTime endedAt)
    {
        if (!CanBeAbandonedBy(abandoningAccountId))
        {
            throw new InvalidOperationException("Only a player of a match in progress can abandon it.");
        }

        foreach (Participation participation in _participations)
        {
            bool isAbandoning = participation.AccountId == abandoningAccountId;
            participation.Finish(isAbandoning ? ParticipationResult.Lost : ParticipationResult.Won);
        }

        Status = MatchStatus.Finished;
        EndedAt = endedAt;
        EndReason = MatchEndReason.Abandonment;
    }

    private void SeatEntrants(IReadOnlyList<MatchEntrant> entrantsInTurnOrder, MatchFormat format)
    {
        ArgumentNullException.ThrowIfNull(entrantsInTurnOrder);
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
