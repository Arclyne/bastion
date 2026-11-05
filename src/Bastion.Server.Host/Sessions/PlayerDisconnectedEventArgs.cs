using System;

namespace Bastion.Server.Host.Sessions;

public sealed class PlayerDisconnectedEventArgs : EventArgs
{
    public PlayerDisconnectedEventArgs(int accountId)
    {
        AccountId = accountId;
    }

    public int AccountId { get; }
}
