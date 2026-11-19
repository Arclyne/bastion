namespace Bastion.Client.Networking.Matches;

public sealed record PlayerStanding
{
    public required byte TurnOrder { get; init; }

    public required StandingResult Result { get; init; }

    public required byte? Placement { get; init; }

    public required EndedMatchReason? EndReason { get; init; }

    public required short? InitialElo { get; init; }

    public required short? FinalElo { get; init; }

    public required byte RemainingWalls { get; init; }
}
