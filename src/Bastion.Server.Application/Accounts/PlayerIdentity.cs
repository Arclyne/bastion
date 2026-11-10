using System;

namespace Bastion.Server.Application.Accounts;

public sealed record PlayerIdentity
{
    public PlayerIdentity(int accountId, string nickname, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(friendCode);

        AccountId = accountId;
        Nickname = nickname;
        FriendCode = friendCode;
    }

    public int AccountId { get; }

    public string Nickname { get; }

    public string FriendCode { get; }
}
