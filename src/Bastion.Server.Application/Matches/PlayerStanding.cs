using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed record PlayerStanding
{
    public required byte TurnOrder { get; init; }

    public required ParticipationResult? Result { get; init; }

    public required byte? Placement { get; init; }

    public required MatchEndReason? EndReason { get; init; }

    public required short? InitialElo { get; init; }

    public required short? FinalElo { get; init; }

    public required byte RemainingWalls { get; init; }
}
