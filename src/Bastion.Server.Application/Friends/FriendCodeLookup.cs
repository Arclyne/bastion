using System;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public static class FriendCodeLookup
{
    public static PlayerIdentity? FindPlayer(IAccountRepository accounts, string enteredCode)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(enteredCode);

        string normalizedCode = FriendCodePolicy.Normalize(enteredCode);
        if (!FriendCodePolicy.IsValid(normalizedCode))
        {
            return null;
        }

        return accounts.FindPlayerIdentityByFriendCode(normalizedCode);
    }
}
