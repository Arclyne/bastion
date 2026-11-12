using System;
using Bastion.Domain.Accounts;

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

    public static PlayerIdentity FromAccount(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode);
    }
}
