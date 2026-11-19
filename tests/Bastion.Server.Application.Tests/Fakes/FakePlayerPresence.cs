using System.Collections.Generic;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePlayerPresence : IPlayerPresence
{
    public FakePlayerPresence()
    {
        ConnectedAccountIds = [];
    }

    public HashSet<int> ConnectedAccountIds { get; }

    public bool IsPlayerConnected(int accountId)
    {
        return ConnectedAccountIds.Contains(accountId);
    }
}
