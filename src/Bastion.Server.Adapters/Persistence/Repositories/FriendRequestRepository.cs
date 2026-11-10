using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bastion.Domain.Accounts;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Friends;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class FriendRequestRepository : IFriendRequestRepository
{
    private readonly BastionDbContext _context;

    public FriendRequestRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public FriendRequest? FindBetween(int firstAccountId, int secondAccountId)
    {
        return _context.FriendRequests.FirstOrDefault(request =>
            (request.RequesterId == firstAccountId && request.RecipientId == secondAccountId)
            || (request.RequesterId == secondAccountId && request.RecipientId == firstAccountId));
    }

    public IReadOnlyList<FriendRequest> ListInvolving(int accountId)
    {
        return _context.FriendRequests
            .AsNoTracking()
            .Where(request => request.RequesterId == accountId || request.RecipientId == accountId)
            .ToList();
    }

    public IReadOnlyList<FriendRequestSummary> ListReceived(int accountId)
    {
        return ListSummaries(
            _context.FriendRequests.Where(request => request.RecipientId == accountId),
            request => request.RequesterId);
    }

    public IReadOnlyList<FriendRequestSummary> ListSent(int accountId)
    {
        return ListSummaries(
            _context.FriendRequests.Where(request => request.RequesterId == accountId),
            request => request.RecipientId);
    }

    public void Add(FriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _context.FriendRequests.Add(request);
    }

    public void Remove(FriendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        int requesterId = request.RequesterId;
        int recipientId = request.RecipientId;
        _context.Database
            .SqlQuery<DeletedRowCount>(
                $"EXEC dbo.usp_FriendRequest_Delete @RequesterId = {requesterId}, @RecipientId = {recipientId}")
            .AsEnumerable()
            .Single();
        _context.Entry(request).State = EntityState.Detached;
    }

    private List<FriendRequestSummary> ListSummaries(
        IQueryable<FriendRequest> requests,
        Expression<Func<FriendRequest, int>> counterpartAccountId)
    {
        return requests
            .Join(
                _context.Accounts.Where(account => account.AccountStatus != AccountStatus.Deleted),
                counterpartAccountId,
                account => account.AccountId,
                (request, account) => new FriendRequestSummary(
                    new PlayerIdentity(account.AccountId, account.Nickname, account.FriendCode),
                    request.RequestedAt,
                    request.RequestedAt.AddDays(FriendshipPolicy.RequestLifetimeDays)))
            .ToList();
    }
}
