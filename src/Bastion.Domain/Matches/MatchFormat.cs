using Bastion.Domain.GameModes;

namespace Bastion.Domain.Matches;

public sealed record MatchFormat(GameMode GameMode, byte ClockMinutes);
