using System.Collections.Generic;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Accounts;

public interface IAccountRepository
{
    Account? FindByIdentifier(string identifier);

    Account? FindById(int accountId);

    PlayerIdentity? FindPlayerIdentity(int accountId);

    PlayerIdentity? FindPlayerIdentityByFriendCode(string friendCode);

    IReadOnlyList<PlayerIdentity> ListPlayerIdentities(IReadOnlyCollection<int> accountIds);

    IReadOnlyList<PlayerIdentity> SearchPlayerIdentities(string nicknamePrefix, int limit);
}
