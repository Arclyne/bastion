using System;
using System.Collections.Generic;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;

namespace Bastion.Server.Application.Matches;

public sealed record MatchClosure
{
    public MatchClosure(
        Match match,
        IReadOnlyList<ModeStatistic> updatedStatistics,
        IReadOnlyList<ModeStatistic> startedStatistics)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(updatedStatistics);
        ArgumentNullException.ThrowIfNull(startedStatistics);

        Match = match;
        UpdatedStatistics = updatedStatistics;
        StartedStatistics = startedStatistics;
    }

    public Match Match { get; }

    public IReadOnlyList<ModeStatistic> UpdatedStatistics { get; }

    public IReadOnlyList<ModeStatistic> StartedStatistics { get; }
}
