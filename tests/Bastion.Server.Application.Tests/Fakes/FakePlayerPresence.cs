using System.Collections.Generic;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePlayerPresence : IPlayerPresence
{
    public FakePlayerPresence()
    {
        ConnectedAccountIds = new HashSet<int>();
    }

    public HashSet<int> ConnectedAccountIds { get; }

    public bool IsPlayerConnected(int accountId)
    {
        return ConnectedAccountIds.Contains(accountId);
    }
}
