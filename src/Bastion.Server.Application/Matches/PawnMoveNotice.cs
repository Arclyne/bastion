namespace Bastion.Server.Application.Matches;

public sealed record PawnMoveNotice
{
    public required string MoverNickname { get; init; }

    public required string FromSquare { get; init; }

    public required string ToSquare { get; init; }

    public required byte NextTurnOrder { get; init; }

    public required short NextMoveNumber { get; init; }

    public required int? RemainingClockMilliseconds { get; init; }

    public required bool IsMatchOver { get; init; }
}
