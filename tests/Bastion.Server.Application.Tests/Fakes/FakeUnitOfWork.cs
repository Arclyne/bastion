using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Auditing;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Moderation;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeAccountRepository AccountStore { get; } = new FakeAccountRepository();

    public FakeSessionRepository SessionStore { get; } = new FakeSessionRepository();

    public FakeAccessLogRepository AccessLogStore { get; } = new FakeAccessLogRepository();

    public FakeFriendshipRepository FriendshipStore { get; } = new FakeFriendshipRepository();

    public FakeFriendRequestRepository FriendRequestStore { get; } = new FakeFriendRequestRepository();

    public FakeSanctionRepository SanctionStore { get; } = new FakeSanctionRepository();

    public FakeBannedWordRepository BannedWordStore { get; } = new FakeBannedWordRepository();

    public FakeMessageRepository MessageStore { get; } = new FakeMessageRepository();

    public int SaveCount { get; private set; }

    public IAccountRepository Accounts => AccountStore;

    public ISessionRepository Sessions => SessionStore;

    public IAccessLogRepository AccessLogs => AccessLogStore;

    public IFriendshipRepository Friendships => FriendshipStore;

    public IFriendRequestRepository FriendRequests => FriendRequestStore;

    public ISanctionRepository Sanctions => SanctionStore;

    public IBannedWordRepository BannedWords => BannedWordStore;

    public IMessageRepository Messages => MessageStore;

    public void SaveChanges()
    {
        SaveCount++;
    }
}
