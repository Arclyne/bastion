using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class DrawEventArgs : EventArgs
{
    public required int MatchId { get; init; }

    public required byte OffererTurnOrder { get; init; }
}
