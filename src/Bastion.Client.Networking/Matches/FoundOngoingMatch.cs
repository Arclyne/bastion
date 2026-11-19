namespace Bastion.Client.Networking.Matches;

public sealed record FoundOngoingMatch
{
    public static FoundOngoingMatch None { get; } = new FoundOngoingMatch();

    public bool IsFound => Match is not null;

    public RejoinedMatch? Match { get; init; }
}
