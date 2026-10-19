using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Auditing;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Common;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }

    ISessionRepository Sessions { get; }

    IAccessLogRepository AccessLogs { get; }

    void SaveChanges();
}
