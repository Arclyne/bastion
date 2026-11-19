using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed record MatchEndRow
{
    public required string Nickname { get; init; }

    public required bool IsOwn { get; init; }

    public required PawnSymbol PawnSymbol { get; init; }

    public required string PlaceText { get; init; }

    public required string ResultText { get; init; }
}
