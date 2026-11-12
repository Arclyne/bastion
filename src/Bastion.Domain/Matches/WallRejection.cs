namespace Bastion.Domain.Matches;

public sealed record WallRejection(WallRejectionReason Reason, byte? BlockedTurnOrder);
