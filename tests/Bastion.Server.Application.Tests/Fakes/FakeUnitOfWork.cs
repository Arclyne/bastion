using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Auditing;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeAccountRepository AccountStore { get; } = new FakeAccountRepository();

    public FakeSessionRepository SessionStore { get; } = new FakeSessionRepository();

    public FakeAccessLogRepository AccessLogStore { get; } = new FakeAccessLogRepository();

    public FakeFriendshipRepository FriendshipStore { get; } = new FakeFriendshipRepository();

    public FakeFriendRequestRepository FriendRequestStore { get; } = new FakeFriendRequestRepository();

    public int SaveCount { get; private set; }

    public IAccountRepository Accounts => AccountStore;

    public ISessionRepository Sessions => SessionStore;

    public IAccessLogRepository AccessLogs => AccessLogStore;

    public IFriendshipRepository Friendships => FriendshipStore;

    public IFriendRequestRepository FriendRequests => FriendRequestStore;

    public void SaveChanges()
    {
        SaveCount++;
    }
}
