using System;
using Bastion.Server.Adapters.Persistence.Repositories;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Auditing;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Adapters.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BastionDbContext _context;

    public UnitOfWork(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        Accounts = new AccountRepository(context);
        Sessions = new SessionRepository(context);
        AccessLogs = new AccessLogRepository(context);
        Friendships = new FriendshipRepository(context);
        FriendRequests = new FriendRequestRepository(context);
    }

    public IAccountRepository Accounts { get; }

    public ISessionRepository Sessions { get; }

    public IAccessLogRepository AccessLogs { get; }

    public IFriendshipRepository Friendships { get; }

    public IFriendRequestRepository FriendRequests { get; }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
