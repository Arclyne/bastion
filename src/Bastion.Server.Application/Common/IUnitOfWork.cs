using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Auditing;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Moderation;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Common;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }

    ISessionRepository Sessions { get; }

    IAccessLogRepository AccessLogs { get; }

    IFriendshipRepository Friendships { get; }

    IFriendRequestRepository FriendRequests { get; }

    ISanctionRepository Sanctions { get; }

    IBannedWordRepository BannedWords { get; }

    IMessageRepository Messages { get; }

    void SaveChanges();
}
