using System;

namespace Bastion.Domain.Matches;

public static class MatchConnectionPolicy
{
    public const int ReturnDelaySeconds = 60;

    public static TimeSpan ReturnDelay { get; } = TimeSpan.FromSeconds(ReturnDelaySeconds);
}
