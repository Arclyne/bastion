using System;
using Bastion.Domain.Common;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public static class FriendCodeLookup
{
    public static Lookup<PlayerIdentity> FindPlayer(IAccountRepository accounts, string enteredCode)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(enteredCode);

        string normalizedCode = FriendCodePolicy.Normalize(enteredCode);
        if (!FriendCodePolicy.IsValid(normalizedCode))
        {
            return Lookup.Missing<PlayerIdentity>($"FriendCode={normalizedCode}");
        }

        return accounts.FindPlayerIdentityByFriendCode(normalizedCode);
    }
}
