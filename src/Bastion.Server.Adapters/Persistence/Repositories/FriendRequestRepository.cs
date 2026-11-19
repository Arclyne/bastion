using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bastion.Domain.Accounts;
using Bastion.Domain.Common;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
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

    public Lookup<FriendRequest> FindBetween(int firstAccountId, int secondAccountId)
    {
        Lookup<FriendRequest> sentByFirst = FindSent(firstAccountId, secondAccountId);
        if (sentByFirst.IsFound)
        {
            return sentByFirst;
        }

        return FindSent(secondAccountId, firstAccountId);
    }

    public IReadOnlyList<FriendRequest> ListInvolving(int accountId)
    {
        IQueryable<FriendRequest> requestsInvolvingAccount = _context.FriendRequests
            .AsNoTracking()
            .Where(request => request.RequesterId == accountId || request.RecipientId == accountId);
        return new List<FriendRequest>(requestsInvolvingAccount);
    }

    public IReadOnlyList<FriendRequestSummary> ListReceived(int accountId)
    {
        IQueryable<FriendRequest> receivedRequests = _context.FriendRequests
            .Where(request => request.RecipientId == accountId);
        Expression<Func<FriendRequest, int>> requesterIdSelector = request => request.RequesterId;
        return ListSummaries(receivedRequests, requesterIdSelector);
    }

    public IReadOnlyList<FriendRequestSummary> ListSent(int accountId)
    {
        IQueryable<FriendRequest> sentRequests = _context.FriendRequests
            .Where(request => request.RequesterId == accountId);
        Expression<Func<FriendRequest, int>> recipientIdSelector = request => request.RecipientId;
        return ListSummaries(sentRequests, recipientIdSelector);
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
        FormattableString command =
            $"EXEC dbo.usp_FriendRequest_Delete @RequesterId = {requesterId}, @RecipientId = {recipientId}";
        DeleteProcedure.Run(_context.Database, command);
        _context.Entry(request).State = EntityState.Detached;
    }

    private Lookup<FriendRequest> FindSent(int requesterId, int recipientId)
    {
        FriendRequest? request = _context.FriendRequests.Find(requesterId, recipientId);
        return Lookup.FromNullable(request, $"RequesterId={requesterId}; RecipientId={recipientId}");
    }

    private List<FriendRequestSummary> ListSummaries(
        IQueryable<FriendRequest> requests,
        Expression<Func<FriendRequest, int>> counterpartAccountIdSelector)
    {
        IQueryable<Account> nonDeletedAccounts = _context.Accounts
            .Where(account => account.AccountStatus != AccountStatus.Deleted);
        IQueryable<RequestWithCounterpart> requestsWithCounterpart = requests
            .AsNoTracking()
            .Join(
                nonDeletedAccounts,
                counterpartAccountIdSelector,
                account => account.AccountId,
                (request, counterpart) => new RequestWithCounterpart(request, counterpart));
        var loadedRequests = new List<RequestWithCounterpart>(requestsWithCounterpart);
        IEnumerable<FriendRequestSummary> summaries = loadedRequests
            .Select(ToSummary);
        return new List<FriendRequestSummary>(summaries);
    }

    private static FriendRequestSummary ToSummary(RequestWithCounterpart requestWithCounterpart)
    {
        var counterpart = PlayerIdentity.FromAccount(requestWithCounterpart.Counterpart);
        FriendRequest request = requestWithCounterpart.Request;
        DateTime expiresAt = request.GetExpirationTime();
        return new FriendRequestSummary(counterpart, request.RequestedAt, expiresAt);
    }

    private sealed record RequestWithCounterpart
    {
        public RequestWithCounterpart(FriendRequest request, Account counterpart)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(counterpart);

            Request = request;
            Counterpart = counterpart;
        }

        public FriendRequest Request { get; }

        public Account Counterpart { get; }
    }
}
