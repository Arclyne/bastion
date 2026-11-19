using System.Collections.Generic;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchConnectionNotifier : IMatchConnectionNotifier
{
    public FakeMatchConnectionNotifier()
    {
        Disconnections = [];
        ReconnectedTurnOrders = [];
        DisconnectionRecipients = [];
    }

    public List<OpponentDisconnection> Disconnections { get; }

    public List<byte> ReconnectedTurnOrders { get; }

    public List<int> DisconnectionRecipients { get; }

    public void NotifyOpponentDisconnected(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        OpponentDisconnection disconnection)
    {
        Disconnections.Add(disconnection);
        DisconnectionRecipients.AddRange(recipientAccountIds);
    }

    public void NotifyOpponentReconnected(int matchId, IReadOnlyCollection<int> recipientAccountIds, byte turnOrder)
    {
        ReconnectedTurnOrders.Add(turnOrder);
    }
}
