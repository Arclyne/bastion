using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeFriendRequestRepository : IFriendRequestRepository
{
    private readonly List<FriendRequest> _requests = new List<FriendRequest>();

    public FriendRequest? FindBetween(int firstAccountId, int secondAccountId)
    {
        return _requests.Find(request => IsBetween(request, firstAccountId, secondAccountId));
    }

    public IReadOnlyList<FriendRequest> ListInvolving(int accountId)
    {
        IEnumerable<FriendRequest> involvingRequests = _requests
            .Where(request => request.RequesterId == accountId || request.RecipientId == accountId);
        return new List<FriendRequest>(involvingRequests);
    }

    public IReadOnlyList<FriendRequestSummary> ListReceived(int accountId)
    {
        IEnumerable<FriendRequestSummary> receivedRequests = _requests
            .Where(request => request.RecipientId == accountId)
            .Select(request => ToSummary(request.RequesterId, request));
        return new List<FriendRequestSummary>(receivedRequests);
    }

    public IReadOnlyList<FriendRequestSummary> ListSent(int accountId)
    {
        IEnumerable<FriendRequestSummary> sentRequests = _requests
            .Where(request => request.RequesterId == accountId)
            .Select(request => ToSummary(request.RecipientId, request));
        return new List<FriendRequestSummary>(sentRequests);
    }

    public void Add(FriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _requests.Add(request);
    }

    public void Remove(FriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _requests.Remove(request);
    }

    private static bool IsBetween(FriendRequest request, int firstAccountId, int secondAccountId)
    {
        bool isFirstToSecond = request.RequesterId == firstAccountId && request.RecipientId == secondAccountId;
        bool isSecondToFirst = request.RequesterId == secondAccountId && request.RecipientId == firstAccountId;
        return isFirstToSecond || isSecondToFirst;
    }

    private static FriendRequestSummary ToSummary(int counterpartAccountId, FriendRequest request)
    {
        var counterpart = new PlayerIdentity(counterpartAccountId, string.Empty, string.Empty);
        DateTime expiresAt = request.GetExpirationTime();
        return new FriendRequestSummary(counterpart, request.RequestedAt, expiresAt);
    }
}
