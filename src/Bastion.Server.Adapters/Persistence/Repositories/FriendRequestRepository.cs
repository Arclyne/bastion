using System;
using System.Collections.Generic;
using System.Linq;
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
}
