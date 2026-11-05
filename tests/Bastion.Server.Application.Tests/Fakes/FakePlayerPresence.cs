using System.Collections.Generic;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePlayerPresence : IPlayerPresence
{
    public HashSet<int> ConnectedAccountIds { get; } = new HashSet<int>();

    public bool IsPlayerConnected(int accountId)
    {
        return ConnectedAccountIds.Contains(accountId);
    }
}
