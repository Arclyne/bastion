namespace Bastion.Domain.Matches;

public sealed record AbandonmentPreview
{
    public required bool IsRanked { get; init; }

    public required short? InitialElo { get; init; }

    public required short? FinalElo { get; init; }

    public required byte? Placement { get; init; }

    public required int ActivePlayerCount { get; init; }
}
