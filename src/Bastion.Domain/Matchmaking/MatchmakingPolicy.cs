using System;
using Bastion.Domain.GameModes;

namespace Bastion.Domain.Matchmaking;

public static class MatchmakingPolicy
{
    public const int PlayersPerMatch = 2;
    public const GameModeCode RankedGameModeCode = GameModeCode.Classic;
    public const byte RankedClockMinutes = 5;

    private const int PresenceCheckMinutes = 15;
    private const int PresenceResponseSeconds = 60;

    public static TimeSpan PresenceCheckInterval { get; } = TimeSpan.FromMinutes(PresenceCheckMinutes);

    public static TimeSpan PresenceResponseWindow { get; } = TimeSpan.FromSeconds(PresenceResponseSeconds);
}
