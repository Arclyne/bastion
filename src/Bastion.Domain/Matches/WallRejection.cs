namespace Bastion.Domain.Matches;

public sealed record WallRejection(WallRejectionReason Reason, byte? BlockedTurnOrder)
{
    public static WallRejection None { get; } = new WallRejection(WallRejectionReason.None, BlockedTurnOrder: null);
}
