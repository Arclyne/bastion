using System.Collections.Generic;
using Bastion.Domain.Accounts;
using Bastion.Domain.Common;

namespace Bastion.Server.Application.Accounts;

public interface IAccountRepository
{
    Lookup<Account> FindByLoginIdentifier(string loginIdentifier);

    Lookup<Account> FindById(int accountId);

    Lookup<PlayerIdentity> FindPlayerIdentity(int accountId);

    Lookup<PlayerIdentity> FindPlayerIdentityByFriendCode(string friendCode);

    IReadOnlyList<PlayerIdentity> ListPlayerIdentities(IReadOnlyCollection<int> accountIds);

    IReadOnlyList<PlayerIdentity> SearchPlayerIdentities(string nicknamePrefix, int limit);
}
