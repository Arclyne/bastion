using System;

namespace Bastion.Domain.Matches;

public static class MatchClockPolicy
{
    public const int OpeningDelaySeconds = 3;

    public static TimeSpan OpeningDelay { get; } = TimeSpan.FromSeconds(OpeningDelaySeconds);
}
