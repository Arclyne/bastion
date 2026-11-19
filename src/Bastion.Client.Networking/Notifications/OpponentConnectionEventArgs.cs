using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class OpponentConnectionEventArgs : EventArgs
{
    public required int MatchId { get; init; }

    public required byte TurnOrder { get; init; }

    public required DateTime OccurredAt { get; init; }
}
