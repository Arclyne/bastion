using System;

namespace Bastion.Server.Application.Sessions;

public sealed class ConnectedPlayer
{
    public ConnectedPlayer(int accountId, string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        AccountId = accountId;
        Nickname = nickname;
    }

    public int AccountId { get; }

    public string Nickname { get; }
}
