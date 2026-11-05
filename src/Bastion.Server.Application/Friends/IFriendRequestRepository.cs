using System.Collections.Generic;
using Bastion.Domain.Friends;

namespace Bastion.Server.Application.Friends;

public interface IFriendRequestRepository
{
    FriendRequest? FindBetween(int firstAccountId, int secondAccountId);

    IReadOnlyList<FriendRequest> ListInvolving(int accountId);

    IReadOnlyList<FriendRequestSummary> ListReceived(int accountId);

    IReadOnlyList<FriendRequestSummary> ListSent(int accountId);

    void Add(FriendRequest request);

    void Remove(FriendRequest request);
}
