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
    public FakeAccountRepository AccountRepository { get; } = new FakeAccountRepository();

    public FakeSessionRepository SessionRepository { get; } = new FakeSessionRepository();

    public FakeAccessLogRepository AccessLogRepository { get; } = new FakeAccessLogRepository();

    public FakeFriendshipRepository FriendshipRepository { get; } = new FakeFriendshipRepository();

    public FakeFriendRequestRepository FriendRequestRepository { get; } = new FakeFriendRequestRepository();

    public FakeSanctionRepository SanctionRepository { get; } = new FakeSanctionRepository();

    public FakeBannedWordRepository BannedWordRepository { get; } = new FakeBannedWordRepository();

    public FakeMessageRepository MessageRepository { get; } = new FakeMessageRepository();

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
