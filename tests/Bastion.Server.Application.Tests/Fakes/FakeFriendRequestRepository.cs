using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendRequestRepository : IFriendRequestRepository
{
    public List<FriendRequest> Requests { get; } = new List<FriendRequest>();

    public List<FriendRequest> RemovedRequests { get; } = new List<FriendRequest>();

    public FriendRequest? FindBetween(int firstAccountId, int secondAccountId)
    {
        return Requests.Find(request =>
            (request.RequesterId == firstAccountId && request.RecipientId == secondAccountId)
            || (request.RequesterId == secondAccountId && request.RecipientId == firstAccountId));
    }

    public IReadOnlyList<FriendRequest> ListInvolving(int accountId)
    {
        return Requests
            .Where(request => request.RequesterId == accountId || request.RecipientId == accountId)
            .ToList();
    }

    public void Add(FriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Requests.Add(request);
    }

    public void Remove(FriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Requests.Remove(request);
        RemovedRequests.Add(request);
    }
}
