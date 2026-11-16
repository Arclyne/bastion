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
    public FakeUnitOfWork()
    {
        AccountRepository = new FakeAccountRepository();
        SessionRepository = new FakeSessionRepository();
        AccessLogRepository = new FakeAccessLogRepository();
        FriendshipRepository = new FakeFriendshipRepository();
        FriendRequestRepository = new FakeFriendRequestRepository();
        SanctionRepository = new FakeSanctionRepository();
        BannedWordRepository = new FakeBannedWordRepository();
        MessageRepository = new FakeMessageRepository();
    }

    public FakeAccountRepository AccountRepository { get; }

    public FakeSessionRepository SessionRepository { get; }

    public FakeAccessLogRepository AccessLogRepository { get; }

    public FakeFriendshipRepository FriendshipRepository { get; }

    public FakeFriendRequestRepository FriendRequestRepository { get; }

    public FakeSanctionRepository SanctionRepository { get; }

    public FakeBannedWordRepository BannedWordRepository { get; }

    public FakeMessageRepository MessageRepository { get; }

    public int SaveCount { get; private set; }

    public IAccountRepository Accounts => AccountRepository;

    public ISessionRepository Sessions => SessionRepository;

    public IAccessLogRepository AccessLogs => AccessLogRepository;

    public IFriendshipRepository Friendships => FriendshipRepository;

    public IFriendRequestRepository FriendRequests => FriendRequestRepository;

    public ISanctionRepository Sanctions => SanctionRepository;

    public IBannedWordRepository BannedWords => BannedWordRepository;

    public IMessageRepository Messages => MessageRepository;

    public void SaveChanges()
    {
        SaveCount++;
    }
}
